using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Customers;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>
/// Write action - always requires confirmation. Delegates the actual insert to the existing
/// IShopCustomerAppService.CreateAsync, which already owns code-uniqueness checking, field length
/// validation, and TenantId assignment (via ShopCustomerManager). This handler never touches
/// ShopCustomer or its repository directly.
/// </summary>
public class CreateCustomerAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopAiLookupResolver _lookupResolver;
    private readonly IShopCustomerAppService _customerAppService;

    public CreateCustomerAiHandler(
        IShopAiCommandValidator validator,
        IShopAiLookupResolver lookupResolver,
        IShopCustomerAppService customerAppService)
    {
        _validator = validator;
        _lookupResolver = lookupResolver;
        _customerAppService = customerAppService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.CreateCustomer;
    public bool IsWriteAction => true;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.CreateCustomer);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopCustomers.Create);

        var payload = ShopAiPayloadSerializer.Deserialize<CreateCustomerAiCommand>(command.Parameters) ?? new CreateCustomerAiCommand();
        var name = payload.Name?.Trim();

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(name)) missingFields.Add("name");
        if (missingFields.Count > 0)
        {
            throw new ShopAiMissingInformationException(missingFields, command.UserFriendlyMessage);
        }

        var warnings = new List<string>(command.Warnings);

        var phone = payload.Phone?.Trim();
        if (!string.IsNullOrWhiteSpace(phone) && !_validator.IsValidPhone(phone))
        {
            warnings.Add("PhoneFormatUnusual");
        }

        var email = payload.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !_validator.IsValidEmail(email))
        {
            warnings.Add("EmailFormatUnusual");
        }

        var existing = await _lookupResolver.ResolveCustomerByNameAsync(name!, cancellationToken);
        if (existing.Found || existing.IsAmbiguous)
        {
            warnings.Add("PossibleDuplicateCustomerName");
        }

        return new ShopAiActionPreviewDto
        {
            Action = ActionType,
            ActionDisplayNameKey = "::AiAction:CreateCustomer",
            IsWriteAction = true,
            RequiresConfirmation = true,
            Warnings = warnings,
            Fields = new List<ShopAiPreviewFieldDto>
            {
                new() { LabelKey = "::Name", Value = name, IsEmpty = false },
                new() { LabelKey = "::Phone", Value = phone, IsEmpty = string.IsNullOrWhiteSpace(phone) },
            },
        };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<CreateCustomerAiCommand>(action.PayloadJson) ?? new CreateCustomerAiCommand();
        var name = payload.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ShopAiExecutionResultDto { Success = false, ErrorCode = "AiMissingInformation", ErrorMessage = "ShopManagement:AiMissingInformation" };
        }

        var dto = new CreateUpdateShopCustomerDto
        {
            Code = GenerateSystemCode(),
            Name = name,
            CustomerType = ShopCustomerType.Individual,
            Phone = string.IsNullOrWhiteSpace(payload.Phone) ? null : payload.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(payload.Email) ? null : payload.Email.Trim(),
            ContactPerson = string.IsNullOrWhiteSpace(payload.ContactPerson) ? null : payload.ContactPerson.Trim(),
            IsActive = true,
        };

        try
        {
            var created = await _customerAppService.CreateAsync(dto);
            return new ShopAiExecutionResultDto
            {
                Success = true,
                ResultMessage = $"Customer '{created.Name}' was created (code {created.Code}).",
                ResultReferenceType = "ShopCustomer",
                ResultReferenceId = created.Id,
                ResultData = JsonSerializer.SerializeToElement(new { customerId = created.Id, code = created.Code, name = created.Name }),
            };
        }
        catch (BusinessException ex) when (ex.Code == "ShopManagement:CustomerCodeAlreadyExists")
        {
            // Astronomically unlikely with a GUID-derived code, but the underlying manager already
            // owns this uniqueness rule - one retry with a freshly generated code is enough.
            dto.Code = GenerateSystemCode();
            var created = await _customerAppService.CreateAsync(dto);
            return new ShopAiExecutionResultDto
            {
                Success = true,
                ResultMessage = $"Customer '{created.Name}' was created (code {created.Code}).",
                ResultReferenceType = "ShopCustomer",
                ResultReferenceId = created.Id,
                ResultData = JsonSerializer.SerializeToElement(new { customerId = created.Id, code = created.Code, name = created.Name }),
            };
        }
        catch (BusinessException ex)
        {
            return new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ex.Code };
        }
    }

    /// <summary>
    /// The AI is only ever allowed to supply names/phones/etc - never a business code. A short
    /// GUID-derived code is system-generated infrastructure, the same way GuidGenerator.Create()
    /// generates the row's Id; it is not "invented" business data.
    /// </summary>
    private static string GenerateSystemCode() => "AI-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
