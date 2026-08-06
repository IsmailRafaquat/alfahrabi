using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiMessageDto : EntityDto<Guid>
{
    public ShopAiMessageRole Role { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? OriginalTranscription { get; set; }
    public ShopAiLanguage DetectedLanguage { get; set; }
    public ShopAiActionType? DetectedAction { get; set; }
    public ShopAiMessageStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ExecutedDate { get; set; }
    public DateTime CreationTime { get; set; }

    public ShopAiActionPreviewDto? Preview { get; set; }
    public ShopAiExecutionResultDto? ExecutionResult { get; set; }
}
