import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopAiActionPreviewDto,
  ShopAiActionType,
  ShopAiAssistantService,
  ShopAiConversationListDto,
  ShopAiExecutionResultDto,
  ShopAiLanguage,
  ShopAiLookupResolutionDto,
  ShopAiMessageRole,
  ShopAiMessageStatus,
  ShopAiResponseDto,
  ShopAiVoiceService,
} from '../../proxy/shop-management/ai-assistant';
import { ConfirmationHelperService } from '../../shared/services/confirmation-helper.service';
import { ShopAiVoiceUploadService } from './shop-ai-voice-upload.service';

interface ChatDisplayMessage {
  id: string;
  role: 'user' | 'assistant';
  text: string;
  status?: ShopAiMessageStatus;
  action?: ShopAiActionType;
  preview?: ShopAiActionPreviewDto;
  executionResult?: ShopAiExecutionResultDto;
  missingFields?: string[];
  ambiguousLookups?: ShopAiLookupResolutionDto[];
  confirmBusy?: boolean;
  isError?: boolean;
}

@Component({
  selector: 'app-shop-ai-assistant',
  standalone: false,
  templateUrl: './shop-ai-assistant.component.html',
  styleUrl: './shop-ai-assistant.component.scss',
})
export class ShopAiAssistantComponent implements OnInit, OnDestroy {
  private readonly assistantService = inject(ShopAiAssistantService);
  private readonly voiceService = inject(ShopAiVoiceService);
  private readonly voiceUploadService = inject(ShopAiVoiceUploadService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly deleteConfirmation = inject(ConfirmationHelperService);

  readonly ShopAiMessageStatus = ShopAiMessageStatus;
  readonly canUseVoice = this.permissions.getGrantedPolicy('ShopManagement.AiAssistant.UseVoice');

  @ViewChild('messagesEnd') messagesEnd?: ElementRef<HTMLDivElement>;

  conversations: ShopAiConversationListDto[] = [];
  loadingConversations = false;

  activeConversationId?: string;
  messages: ChatDisplayMessage[] = [];
  loadingMessages = false;

  composerText = '';
  sending = false;

  recording = false;
  recordingSeconds = 0;
  transcribing = false;
  pendingTranscription?: { text: string; isLowConfidence: boolean; detectedLanguage: ShopAiLanguage };

  private mediaRecorder?: MediaRecorder;
  private audioChunks: Blob[] = [];
  private recordingTimer?: ReturnType<typeof setInterval>;
  private recordingCancelled = false;
  private mediaStream?: MediaStream;

  ngOnInit(): void {
    this.loadConversations();
  }

  ngOnDestroy(): void {
    this.stopMediaStream();
    if (this.recordingTimer) clearInterval(this.recordingTimer);
  }

  loadConversations(): void {
    this.loadingConversations = true;
    this.assistantService
      .getConversations({ maxResultCount: 100 })
      .pipe(finalize(() => (this.loadingConversations = false)))
      .subscribe({
        next: result => {
          this.conversations = result.items || [];
          if (!this.activeConversationId && this.conversations.length) {
            this.selectConversation(this.conversations[0]);
          }
        },
        error: e => this.showError(e),
      });
  }

  newConversation(): void {
    this.assistantService.createConversation().subscribe({
      next: conv => {
        this.conversations = [{ id: conv.id, title: conv.title, status: conv.status, lastMessageDate: conv.lastMessageDate, creationTime: conv.creationTime }, ...this.conversations];
        this.activeConversationId = conv.id;
        this.messages = [];
        this.resetComposer();
      },
      error: e => this.showError(e),
    });
  }

  selectConversation(conv: ShopAiConversationListDto): void {
    this.activeConversationId = conv.id;
    this.pendingTranscription = undefined;
    this.loadingMessages = true;
    this.assistantService
      .getConversation(conv.id)
      .pipe(finalize(() => (this.loadingMessages = false)))
      .subscribe({
        next: full => {
          this.messages = (full.messages || []).map(m => ({
            id: m.id,
            role: m.role === ShopAiMessageRole.User ? 'user' : 'assistant',
            text: m.messageText,
            status: m.status,
            action: m.detectedAction,
            executionResult: m.executionResult,
            isError: m.status === ShopAiMessageStatus.Failed,
          }));
          this.scrollToBottom();
        },
        error: e => this.showError(e),
      });
  }

  deleteConversation(conv: ShopAiConversationListDto, event: Event): void {
    event.stopPropagation();
    this.deleteConfirmation.confirmDelete().subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.assistantService.deleteConversation(conv.id).subscribe({
        next: () => {
          this.conversations = this.conversations.filter(x => x.id !== conv.id);
          if (this.activeConversationId === conv.id) {
            this.activeConversationId = undefined;
            this.messages = [];
            if (this.conversations.length) this.selectConversation(this.conversations[0]);
          }
        },
        error: e => this.showError(e),
      });
    });
  }

  send(): void {
    const text = this.composerText.trim();
    if (!text || this.sending) return;

    if (!this.activeConversationId) {
      this.assistantService.createConversation().subscribe({
        next: conv => {
          this.conversations = [{ id: conv.id, title: conv.title, status: conv.status, lastMessageDate: conv.lastMessageDate, creationTime: conv.creationTime }, ...this.conversations];
          this.activeConversationId = conv.id;
          this.sendToConversation(text);
        },
        error: e => this.showError(e),
      });
      return;
    }

    this.sendToConversation(text);
  }

  private sendToConversation(text: string): void {
    const conversationId = this.activeConversationId!;
    this.messages.push({ id: 'local-' + Date.now(), role: 'user', text });
    this.composerText = '';
    this.sending = true;
    this.scrollToBottom();

    this.assistantService
      .sendMessage({ conversationId, message: text })
      .pipe(finalize(() => (this.sending = false)))
      .subscribe({
        next: response => this.appendAssistantResponse(response),
        error: e => {
          this.messages.push({ id: 'error-' + Date.now(), role: 'assistant', text: this.errorText(e), isError: true });
          this.scrollToBottom();
        },
      });
    this.touchConversationOrder(conversationId);
  }

  private appendAssistantResponse(response: ShopAiResponseDto): void {
    this.messages.push({
      id: response.messageId,
      role: 'assistant',
      text: response.assistantMessage || '',
      status: response.status,
      action: response.action,
      preview: response.preview,
      executionResult: response.executionResult,
      missingFields: response.missingFields,
      ambiguousLookups: response.ambiguousLookups,
      isError: response.status === ShopAiMessageStatus.Failed,
    });
    this.scrollToBottom();
  }

  confirmAction(msg: ChatDisplayMessage): void {
    if (!this.activeConversationId || !msg.preview?.confirmationToken || msg.confirmBusy) return;
    msg.confirmBusy = true;

    this.assistantService
      .confirmAction({ conversationId: this.activeConversationId, messageId: msg.id, confirmationToken: msg.preview.confirmationToken })
      .pipe(finalize(() => (msg.confirmBusy = false)))
      .subscribe({
        next: result => {
          msg.executionResult = result;
          msg.status = result.success ? ShopAiMessageStatus.Executed : ShopAiMessageStatus.Failed;
          msg.isError = !result.success;
          msg.preview = undefined;
        },
        error: e => {
          msg.isError = true;
          msg.executionResult = { success: false, errorMessage: this.errorText(e) };
          msg.preview = undefined;
        },
      });
  }

  cancelActionOnMessage(msg: ChatDisplayMessage): void {
    if (!this.activeConversationId || msg.confirmBusy) return;
    msg.confirmBusy = true;
    this.assistantService
      .cancelAction({ conversationId: this.activeConversationId, messageId: msg.id })
      .pipe(finalize(() => (msg.confirmBusy = false)))
      .subscribe({
        next: () => {
          msg.status = ShopAiMessageStatus.Cancelled;
          msg.preview = undefined;
        },
        error: e => this.showError(e),
      });
  }

  // ------------------------------------------------------------------
  // Voice
  // ------------------------------------------------------------------

  async startRecording(): Promise<void> {
    if (this.recording) return;
    try {
      this.mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    } catch {
      this.toaster.error('::AiAssistant.MicrophoneUnavailable');
      return;
    }

    this.recordingCancelled = false;
    this.audioChunks = [];
    const mimeType = MediaRecorder.isTypeSupported('audio/webm') ? 'audio/webm' : '';
    this.mediaRecorder = mimeType ? new MediaRecorder(this.mediaStream, { mimeType }) : new MediaRecorder(this.mediaStream);
    this.mediaRecorder.ondataavailable = e => { if (e.data.size > 0) this.audioChunks.push(e.data); };
    this.mediaRecorder.onstop = () => this.onRecordingStopped();
    this.mediaRecorder.start();

    this.recording = true;
    this.recordingSeconds = 0;
    this.recordingTimer = setInterval(() => (this.recordingSeconds += 1), 1000);
  }

  stopRecording(): void {
    if (!this.recording) return;
    this.recording = false;
    if (this.recordingTimer) clearInterval(this.recordingTimer);
    this.mediaRecorder?.stop();
  }

  cancelRecording(): void {
    this.recordingCancelled = true;
    this.stopRecording();
  }

  private onRecordingStopped(): void {
    this.stopMediaStream();
    if (this.recordingCancelled || this.audioChunks.length === 0) return;

    const blob = new Blob(this.audioChunks, { type: this.mediaRecorder?.mimeType || 'audio/webm' });
    this.transcribing = true;
    this.voiceUploadService.transcribe(blob, 'recording.webm').subscribe({
      next: result => {
        this.transcribing = false;
        this.pendingTranscription = { text: result.text, isLowConfidence: result.isLowConfidence, detectedLanguage: result.detectedLanguage };
      },
      error: e => {
        this.transcribing = false;
        this.showError(e);
      },
    });
  }

  private stopMediaStream(): void {
    this.mediaStream?.getTracks().forEach(t => t.stop());
    this.mediaStream = undefined;
  }

  acceptTranscription(): void {
    if (!this.pendingTranscription) return;
    const accepted = this.pendingTranscription.text.trim();
    const original = this.pendingTranscription.text;
    this.pendingTranscription = undefined;
    if (!accepted) return;

    if (!this.activeConversationId) {
      this.assistantService.createConversation().subscribe({
        next: conv => {
          this.conversations = [{ id: conv.id, title: conv.title, status: conv.status, lastMessageDate: conv.lastMessageDate, creationTime: conv.creationTime }, ...this.conversations];
          this.activeConversationId = conv.id;
          this.sendVoiceToConversation(accepted, original);
        },
        error: e => this.showError(e),
      });
      return;
    }

    this.sendVoiceToConversation(accepted, original);
  }

  private sendVoiceToConversation(accepted: string, original: string): void {
    const conversationId = this.activeConversationId!;
    this.messages.push({ id: 'local-' + Date.now(), role: 'user', text: accepted });
    this.sending = true;
    this.scrollToBottom();

    this.voiceService
      .sendVoiceMessage({ conversationId, acceptedText: accepted, originalTranscription: original })
      .pipe(finalize(() => (this.sending = false)))
      .subscribe({
        next: response => this.appendAssistantResponse(response),
        error: e => {
          this.messages.push({ id: 'error-' + Date.now(), role: 'assistant', text: this.errorText(e), isError: true });
          this.scrollToBottom();
        },
      });
    this.touchConversationOrder(conversationId);
  }

  recordAgain(): void {
    this.pendingTranscription = undefined;
    this.startRecording();
  }

  discardTranscription(): void {
    this.pendingTranscription = undefined;
  }

  // ------------------------------------------------------------------
  // Helpers
  // ------------------------------------------------------------------

  onComposerKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  private resetComposer(): void {
    this.composerText = '';
    this.pendingTranscription = undefined;
  }

  private touchConversationOrder(conversationId: string): void {
    const idx = this.conversations.findIndex(c => c.id === conversationId);
    if (idx > 0) {
      const [conv] = this.conversations.splice(idx, 1);
      this.conversations.unshift(conv);
    }
  }

  private scrollToBottom(): void {
    setTimeout(() => this.messagesEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' }), 50);
  }

  private showError(e: any): void {
    this.toaster.error(this.errorText(e));
  }

  private errorText(e: any): string {
    return e?.error?.error?.message || e?.message || '::UnexpectedError';
  }
}
