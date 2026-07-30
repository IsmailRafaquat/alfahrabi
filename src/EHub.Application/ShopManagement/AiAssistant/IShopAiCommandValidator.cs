using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// A toolkit of small, reusable validation primitives that each IShopAiActionHandler.PrepareAsync
/// calls while building its typed command and preview. It does not itself dispatch by action type -
/// each handler owns the specific fields it needs to check. Final, authoritative validation (string
/// lengths, uniqueness, business rules) still happens inside the existing Domain Manager /
/// AppService the handler calls in ExecuteAsync - this toolkit only avoids sending an obviously
/// incomplete or malformed request that far.
/// </summary>
public interface IShopAiCommandValidator
{
    Guid RequireTenant();
    Guid RequireUser();

    /// <summary>Throws AbpAuthorizationException if the current user lacks the given permission.</summary>
    Task EnsurePermissionAsync(string permissionName);

    bool IsValidPhone(string? phone);
    bool IsValidEmail(string? email);
}
