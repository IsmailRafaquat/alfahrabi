using System;
using Volo.Abp.Application.Dtos;
namespace EHub.ShopManagement.Units;
public class ShopUnitDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string ShortName { get; set; } = string.Empty; public bool AllowDecimal { get; set; } public bool IsActive { get; set; } public DateTime CreationTime { get; set; } }
