using System;

namespace EHub.ShopManagement.ProductBatches;

/// <summary>
/// A user-chosen (manual) batch allocation line, decoupled from Application.Contracts DTOs so the
/// domain manager does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopBatchAllocationInput
{
    public Guid ProductBatchId { get; set; }
    public decimal Quantity { get; set; }
}
