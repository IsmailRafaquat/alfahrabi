import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopAiActionType } from './shop-ai-action-type.enum';
import type { ShopAiLanguage } from './shop-ai-language.enum';
import type { ShopAiConversationStatus } from './shop-ai-conversation-status.enum';
import type { ShopAiMessageRole } from './shop-ai-message-role.enum';
import type { ShopAiMessageStatus } from './shop-ai-message-status.enum';
import type { ShopAiResponseType } from './shop-ai-response-type.enum';
import type { IRemoteStreamContent } from '../../volo/abp/content/models';

export interface CancelShopAiActionDto {
  conversationId: string;
  messageId: string;
}

export interface ConfirmShopAiActionDto {
  conversationId: string;
  messageId: string;
  confirmationToken: string;
}

export interface GetShopAiConversationsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface SendShopAiMessageDto {
  conversationId: string;
  message: string;
}

export interface ShopAiActionPreviewDto {
  action?: ShopAiActionType;
  actionDisplayNameKey?: string;
  isWriteAction: boolean;
  requiresConfirmation: boolean;
  fields: ShopAiPreviewFieldDto[];
  warnings: string[];
  noteKey?: string;
  confirmationToken?: string;
  confirmationExpiryDate?: string;
}

export interface ShopAiConversationDto extends EntityDto<string> {
  title?: string;
  detectedLanguage?: ShopAiLanguage;
  status?: ShopAiConversationStatus;
  lastMessageDate?: string;
  creationTime?: string;
  messages: ShopAiMessageDto[];
}

export interface ShopAiConversationListDto extends EntityDto<string> {
  title?: string;
  status?: ShopAiConversationStatus;
  lastMessageDate?: string;
  creationTime?: string;
}

export interface ShopAiDataListDto {
  title?: string;
  totalCount: number;
  rows: Record<string, object>[];
}

export interface ShopAiExecutionResultDto {
  success: boolean;
  resultMessage?: string;
  resultData?: any;
  dataList?: ShopAiDataListDto;
  resultReferenceType?: string;
  resultReferenceId?: string;
  errorCode?: string;
  errorMessage?: string;
}

export interface ShopAiFieldDescriptionDto {
  fieldKey?: string;
  displayName?: string;
  description?: string;
  dataType?: string;
  isRequired: boolean;
  isLookup: boolean;
  isSystemGenerated: boolean;
  exampleValue?: string;
  allowedValues: string[];
}

export interface ShopAiGuidedCreationProgressDto {
  moduleKey?: string;
  moduleDisplayName?: string;
  collectedFields: ShopAiPreviewFieldDto[];
  missingRequiredFields: string[];
  requiredFieldCount: number;
  completedRequiredFieldCount: number;
}

export interface ShopAiLookupChoiceDto {
  id?: string;
  displayText?: string;
}

export interface ShopAiLookupResolutionDto {
  fieldName?: string;
  query?: string;
  choices: ShopAiLookupChoiceDto[];
}

export interface ShopAiMessageDto extends EntityDto<string> {
  role?: ShopAiMessageRole;
  messageText?: string;
  originalTranscription?: string;
  detectedLanguage?: ShopAiLanguage;
  detectedAction?: ShopAiActionType;
  status?: ShopAiMessageStatus;
  errorCode?: string;
  errorMessage?: string;
  executedDate?: string;
  creationTime?: string;
  preview: ShopAiActionPreviewDto;
  executionResult: ShopAiExecutionResultDto;
}

export interface ShopAiModuleExplanationDto {
  moduleKey?: string;
  displayName?: string;
  description?: string;
  supportsCreation: boolean;
  createsDraftOnly: boolean;
  requiredFields: ShopAiFieldDescriptionDto[];
  optionalFields: ShopAiFieldDescriptionDto[];
  systemGeneratedFields: ShopAiFieldDescriptionDto[];
  businessRules: string[];
  relatedModules: string[];
}

export interface ShopAiModuleListDto {
  moduleKey?: string;
  displayName?: string;
  description?: string;
  supportsCreation: boolean;
}

export interface ShopAiPreviewFieldDto {
  labelKey?: string;
  value?: string;
  isEmpty: boolean;
}

export interface ShopAiResponseDto {
  conversationId?: string;
  messageId?: string;
  status?: ShopAiMessageStatus;
  action?: ShopAiActionType;
  detectedLanguage?: ShopAiLanguage;
  assistantMessage?: string;
  missingFields: string[];
  warnings: string[];
  preview: ShopAiActionPreviewDto;
  executionResult: ShopAiExecutionResultDto;
  dataList: ShopAiDataListDto;
  ambiguousLookups: ShopAiLookupResolutionDto[];
  errorCode?: string;
  errorMessage?: string;
  responseType?: ShopAiResponseType;
  moduleKey?: string;
  module: ShopAiModuleExplanationDto;
  fields: ShopAiFieldDescriptionDto[];
  guidedCreationProgress: ShopAiGuidedCreationProgressDto;
}

export interface ShopAiVoiceMessageDto {
  conversationId: string;
  acceptedText: string;
  originalTranscription?: string;
  detectedLanguage?: ShopAiLanguage;
}

export interface ShopAiVoiceTranscriptionDto {
  text?: string;
  detectedLanguage?: ShopAiLanguage;
  languageProbability: number;
  durationSeconds: number;
  isLowConfidence: boolean;
}

export interface ShopAiVoiceUploadDto {
  audio: IRemoteStreamContent;
  languageHint?: string;
}
