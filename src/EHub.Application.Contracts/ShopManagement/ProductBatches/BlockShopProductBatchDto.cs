using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.ProductBatches;

public class BlockShopProductBatchDto
{
    [Required, StringLength(ShopProductBatchConsts.BlockReasonMaxLength)]
    public string BlockReason { get; set; } = string.Empty;
}
