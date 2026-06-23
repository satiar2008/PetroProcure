namespace PetroProcure.Application.Mesc;

public sealed class MescCatalogOptions
{
    public const string SectionName = "MescCatalog";

    public bool AutoCreateGeneralGroup { get; set; }

    public bool AllowNonNumericItemCodes { get; set; }
}
