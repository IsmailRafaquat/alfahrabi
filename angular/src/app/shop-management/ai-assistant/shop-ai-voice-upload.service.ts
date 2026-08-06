import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ShopAiVoiceTranscriptionDto } from '../../proxy/shop-management/ai-assistant';

/**
 * The generated ShopAiVoiceService.transcribe() sends its body as JSON, but the backend's
 * ShopAiVoiceUploadDto.Audio is an IRemoteStreamContent - it needs an actual multipart/form-data
 * upload, which ABP's Angular proxy generator does not produce for that parameter shape. This
 * bypasses the generated proxy for just this one call and posts the recording as real multipart
 * form data instead. ABP's global auth interceptor still attaches the bearer token to this
 * request since it intercepts HttpClient app-wide, not just RestService-issued calls.
 */
@Injectable({ providedIn: 'root' })
export class ShopAiVoiceUploadService {
  constructor(private http: HttpClient) {}

  transcribe(audio: Blob, fileName: string, languageHint?: string): Observable<ShopAiVoiceTranscriptionDto> {
    const formData = new FormData();
    formData.append('Audio', audio, fileName);
    if (languageHint) formData.append('LanguageHint', languageHint);

    return this.http.post<ShopAiVoiceTranscriptionDto>(
      `${environment.apis.default.url}/api/app/shop-ai-voice/transcribe`,
      formData,
    );
  }
}
