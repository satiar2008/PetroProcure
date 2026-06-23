using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Indents;

public sealed class IndentItem : AuditableEntity<Guid>
{
    public IndentItem(
        Guid id,
        Guid indentId,
        Guid mescItemId,
        string mescCode,
        string mescGeneralGroupCode,
        string generalDescription,
        string specificDescription,
        Guid unitOfMeasureId,
        decimal requestedQuantity,
        string? technicalDescription = null,
        DateOnly? requiredDate = null)
        : base(id)
    {
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        IndentId = indentId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        RequestedQuantity = requestedQuantity;
        TechnicalDescription = technicalDescription?.Trim();
        RequiredDate = requiredDate;
    }

    public Guid IndentId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal RequestedQuantity { get; private set; }
    public string? TechnicalDescription { get; private set; }
    public DateOnly? RequiredDate { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
