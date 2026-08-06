using System.Collections.Generic;
using System.Linq;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Generic, module-agnostic sentence templates used to keep the assistant's short lead-in text
/// (never the full field dump - that lives only in the structured DTOs) in the user's detected
/// language. Field-specific wording (e.g. Unit's exact "Kya decimal quantity allow karni hai?")
/// comes from ShopAiFieldMetadata.AskPrompt* instead; these templates are the fallback for every
/// field/module that hasn't been given a hand-written translation yet.
/// </summary>
public static class ShopAiPhrases
{
    public static string ModuleShortIntro(ShopAiLanguage language, string moduleDisplayName) => language switch
    {
        ShopAiLanguage.Urdu => $"{moduleDisplayName} کے بارے میں معلومات:",
        ShopAiLanguage.RomanUrdu => $"{moduleDisplayName} ke baare mein maloomat:",
        _ => $"Here's what you need to know about {moduleDisplayName}:",
    };

    public static string FieldShortIntro(ShopAiLanguage language, string fieldDisplayName) => language switch
    {
        ShopAiLanguage.Urdu => $"{fieldDisplayName}:",
        ShopAiLanguage.RomanUrdu => $"{fieldDisplayName}:",
        _ => $"About the {fieldDisplayName} field:",
    };

    public static string RequiredFieldsIntro(ShopAiLanguage language, string moduleDisplayName, IReadOnlyList<string> requiredFieldNames)
    {
        var bullets = string.Join("\n", requiredFieldNames.Select(f => $"- {f}"));
        return language switch
        {
            ShopAiLanguage.Urdu => $"{moduleDisplayName} بنانے کے لیے یہ تفصیلات درکار ہیں:\n{bullets}",
            ShopAiLanguage.RomanUrdu => $"{moduleDisplayName} create karne ke liye yeh details required hain:\n{bullets}",
            _ => $"To create a {moduleDisplayName}, the following details are required:\n{bullets}",
        };
    }

    public static string AskField(ShopAiLanguage language, ShopAiFieldMetadata field)
    {
        var custom = field.GetAskPrompt(language);
        if (!string.IsNullOrWhiteSpace(custom)) return custom!;

        if (field.DataType == "Boolean")
        {
            return language switch
            {
                ShopAiLanguage.Urdu => $"کیا {field.DisplayName} فعال ہو؟ (ہاں/نہیں)",
                ShopAiLanguage.RomanUrdu => $"Kya {field.DisplayName} chahiye? (Yes/No)",
                _ => $"{field.DisplayName}? (Yes/No)",
            };
        }

        var example = string.IsNullOrWhiteSpace(field.ExampleValue) ? string.Empty : language switch
        {
            ShopAiLanguage.Urdu => $" (مثال: {field.ExampleValue})",
            ShopAiLanguage.RomanUrdu => $" (misal: {field.ExampleValue})",
            _ => $" (e.g. {field.ExampleValue})",
        };

        return language switch
        {
            ShopAiLanguage.Urdu => $"براہ کرم {field.DisplayName} بتائیں۔{example}",
            ShopAiLanguage.RomanUrdu => $"{field.DisplayName} bata dein.{example}",
            _ => $"Please provide {field.DisplayName}.{example}",
        };
    }

    public static string RequiredFieldsCollectedLabel(ShopAiLanguage language) => language switch
    {
        ShopAiLanguage.Urdu => "لازمی فیلڈز جمع ہو گئیں",
        ShopAiLanguage.RomanUrdu => "required fields collected",
        _ => "required fields collected",
    };

    public static string RecordCreatedSuccess(ShopAiLanguage language, string moduleDisplayName, string recordDisplayName) => language switch
    {
        ShopAiLanguage.Urdu => $"{moduleDisplayName} “{recordDisplayName}” کامیابی سے بن گیا۔",
        ShopAiLanguage.RomanUrdu => $"{moduleDisplayName} “{recordDisplayName}” successfully create ho gaya.",
        _ => $"{moduleDisplayName} “{recordDisplayName}” has been created successfully.",
    };
}
