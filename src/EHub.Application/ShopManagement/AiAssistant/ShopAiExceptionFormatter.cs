using System.Collections;
using Microsoft.Extensions.Localization;
using Volo.Abp;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// ABP's automatic exception-message localization (AbpExceptionLocalizationOptions) only fires for
/// exceptions that are actually thrown through the HTTP pipeline. Every AI handler/AppService catch
/// block here turns a caught BusinessException into a plain 200-OK DTO field instead of rethrowing
/// it, so that automatic substitution of placeholders like "{Name}" from ex.Data never runs - this
/// is the one place that substitution is replicated manually, so a message like
/// "ShopManagement:UnitNameAlreadyExists" -&gt; "A unit named '{Name}' already exists." actually gets
/// "Ai" substituted in instead of being shown to the user with the literal placeholder still in it.
/// </summary>
public static class ShopAiExceptionFormatter
{
    public static string Format(IStringLocalizer localizer, BusinessException ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Code))
        {
            return ex.Message;
        }

        var text = localizer[ex.Code].Value;
        foreach (DictionaryEntry entry in ex.Data)
        {
            text = text.Replace("{" + entry.Key + "}", entry.Value?.ToString() ?? string.Empty);
        }
        return text;
    }
}
