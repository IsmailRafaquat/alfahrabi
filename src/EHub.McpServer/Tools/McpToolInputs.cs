using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EHub.McpServer.Tools;

public class PrepareCreateUnitMcpInput
{
    [Required]
    [Description("The unit's full name, e.g. Piece, Kilogram, Meter.")]
    public string Name { get; set; } = default!;

    [Required]
    [Description("A short abbreviation shown throughout the app, e.g. Pc, Kg, M.")]
    public string ShortName { get; set; } = default!;

    [Required]
    [Description("Whether products in this unit can be sold/stocked in fractional amounts (e.g. 1.5). No for whole-count units like Piece; Yes for weight/length/volume units like Kilogram or Meter.")]
    public bool AllowDecimal { get; set; }

    [Description("Whether the unit is selectable for new products. Defaults to true.")]
    public bool IsActive { get; set; } = true;
}

public class PrepareCreateCustomerMcpInput
{
    [Required]
    [Description("The customer's display name.")]
    public string Name { get; set; } = default!;

    [Description("Phone number, if the user provided one.")]
    public string? Phone { get; set; }

    [Description("Email address, if the user provided one.")]
    public string? Email { get; set; }

    [Description("A contact person's name, if the user provided one.")]
    public string? ContactPerson { get; set; }
}

/// <summary>
/// Common confirm-tool input for every write action - deliberately carries no editable field
/// values. The server already has the validated payload stored against PendingActionId from the
/// matching prepare call; re-sending it here would let a caller substitute different values than
/// what was actually previewed and approved.
/// </summary>
public class ConfirmMcpActionInput
{
    [Required]
    [Description("The PendingActionId returned by the matching shop_prepare_* tool call.")]
    public Guid PendingActionId { get; set; }

    [Required]
    [Description("The ConfirmationToken returned by the matching shop_prepare_* tool call.")]
    public string ConfirmationToken { get; set; } = default!;
}
