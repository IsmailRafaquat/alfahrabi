using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiConversationDto : EntityDto<Guid>
{
    public string? Title { get; set; }
    public ShopAiLanguage DetectedLanguage { get; set; }
    public ShopAiConversationStatus Status { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public DateTime CreationTime { get; set; }
    public List<ShopAiMessageDto> Messages { get; set; } = new();
}

public class ShopAiConversationListDto : EntityDto<Guid>
{
    public string? Title { get; set; }
    public ShopAiConversationStatus Status { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public DateTime CreationTime { get; set; }
}
