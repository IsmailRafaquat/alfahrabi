namespace EHub.ShopManagement.PrintSettings;

// CSS presentation hint only (font-weight / letter-spacing on the receipt) - browsers cannot
// control real thermal-printhead burn density, so this never reaches actual printer hardware.
public enum ShopThermalPrintDensity
{
    Light = 0,
    Normal = 1,
    Dark = 2
}
