using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFileItem : AuditableEntity<Guid>
{
    public PurchaseFileItem(
        Guid id, Guid purchaseFileId, Guid mescItemId, string mescCode,
        string mescGeneralGroupCode, string generalDescription, string specificDescription,
        Guid unitOfMeasureId, decimal requestedQuantity, decimal approvedQuantity,
        string? technicalDescription, Guid? sourceIndentItemId)
        : base(id)
    {
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        if (approvedQuantity < 0 || approvedQuantity > requestedQuantity)
            throw new ArgumentOutOfRangeException(nameof(approvedQuantity));
        PurchaseFileId = purchaseFileId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        RequestedQuantity = requestedQuantity;
        ApprovedQuantity = approvedQuantity;
        TechnicalDescription = technicalDescription?.Trim();
        SourceIndentItemId = sourceIndentItemId;
    }

    public Guid PurchaseFileId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal RequestedQuantity { get; private set; }
    public decimal ApprovedQuantity { get; private set; }
    public string? TechnicalDescription { get; private set; }
    public Guid? SourceIndentItemId { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
