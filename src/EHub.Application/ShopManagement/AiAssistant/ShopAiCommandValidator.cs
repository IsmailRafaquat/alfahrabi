using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiCommandValidator : IShopAiCommandValidator, ITransientDependency
{
    private static readonly Regex PhoneRegex = new(@"^[0-9+\-\s()]{6,32}$", RegexOptions.None, TimeSpan.FromSeconds(1));
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, TimeSpan.FromSeconds(1));

    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService;

    public ShopAiCommandValidator(ICurrentTenant currentTenant, ICurrentUser currentUser, IAuthorizationService authorizationService)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _authorizationService = authorizationService;
    }

    public Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    public Guid RequireUser() => _currentUser.Id ?? throw new AbpAuthorizationException("ShopManagement:AiPermissionDenied");

    public async Task EnsurePermissionAsync(string permissionName)
    {
        await _authorizationService.CheckAsync(permissionName);
    }

    public bool IsValidPhone(string? phone) => !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone.Trim());

    public bool IsValidEmail(string? email) => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());
}
