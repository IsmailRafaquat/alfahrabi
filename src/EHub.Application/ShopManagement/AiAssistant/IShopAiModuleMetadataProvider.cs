using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiModuleMetadataProvider
{
    ShopAiModuleMetadata GetModule(string moduleKey);

    IReadOnlyList<ShopAiModuleMetadata> GetModules();

    bool TryGetModule(string moduleNameOrAlias, out ShopAiModuleMetadata? metadata);
}
