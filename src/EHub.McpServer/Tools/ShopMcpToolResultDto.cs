using System;

namespace EHub.McpServer.Tools;

/// <summary>Consistent structured result for every tool - never an internal exception, stack trace, or raw SQL/connection detail.</summary>
public class ShopMcpToolResultDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? ErrorCode { get; set; }
    public T? Data { get; set; }
    public bool RequiresConfirmation { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid? MessageId { get; set; }
    public Guid? PendingActionId { get; set; }
    public string? ConfirmationToken { get; set; }
    public DateTime? ConfirmationExpiryDate { get; set; }

    public static ShopMcpToolResultDto<T> Ok(T data, string message) =>
        new() { Success = true, Data = data, Message = message };

    public static ShopMcpToolResultDto<T> Fail(string errorCode, string message) =>
        new() { Success = false, ErrorCode = errorCode, Message = message };
}
