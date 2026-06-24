using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class InquiryItem : Entity<Guid>
{
    private InquiryItem() : base(Guid.Empty)
    {
        MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty;
    }

    public InquiryItem(Guid id, Guid inquiryId, Guid? purchaseFileItemId, Guid mescItemId, string mescCode,
        string mescGeneralGroupCode, string generalDescription, string specificDescription, Guid unitOfMeasureId,
        decimal requestedQuantity, string? technicalDescription)
        : base(id)
    {
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        InquiryId = inquiryId;
        PurchaseFileItemId = purchaseFileItemId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        RequestedQuantity = requestedQuantity;
        TechnicalDescription = string.IsNullOrWhiteSpace(technicalDescription) ? null : technicalDescription.Trim();
    }

    public Guid InquiryId { get; private set; }
    public Guid? PurchaseFileItemId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal RequestedQuantity { get; private set; }
    public string? TechnicalDescription { get; private set; }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
