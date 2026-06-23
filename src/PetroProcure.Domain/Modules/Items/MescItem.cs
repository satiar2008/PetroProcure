using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Items;

public sealed class MescItem : AuditableEntity<Guid>
{
    public MescItem(Guid id, string code, string description, string unitOfMeasure)
        : base(id)
    {
        Code = ValidateCode(code);
        GeneralGroupCode = GetGeneralGroupCode(Code);
        Description = ValidateRequired(description, "Specific description is required.", nameof(description));
        UnitOfMeasure = ValidateRequired(unitOfMeasure, "Unit of measure is required.", nameof(unitOfMeasure));
        IsActive = true;
    }

    public string Code { get; private set; }

    public string GeneralGroupCode { get; private set; }

    public string Description { get; private set; }

    public string UnitOfMeasure { get; private set; }

    public MescGeneralGroup? GeneralGroup { get; private set; }

    public bool IsActive { get; private set; }

    public static string GetGeneralGroupCode(string code)
    {
        return ValidateCode(code)[..6];
    }

    public void LinkGeneralGroup(MescGeneralGroup generalGroup)
    {
        ArgumentNullException.ThrowIfNull(generalGroup);

        if (!string.Equals(GeneralGroupCode, generalGroup.Code, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("MESC item can only be linked to its matching general group.");
        }

        GeneralGroup = generalGroup;
    }

    public void Update(string code, string description, string unitOfMeasure)
    {
        Code = ValidateCode(code);
        GeneralGroupCode = GetGeneralGroupCode(Code);
        Description = ValidateRequired(description, "Specific description is required.", nameof(description));
        UnitOfMeasure = ValidateRequired(unitOfMeasure, "Unit of measure is required.", nameof(unitOfMeasure));
        MarkModified(null);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public static string ValidateCode(string code, bool allowNonNumeric = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MESC code is required.", nameof(code));
        }

        var normalizedCode = code.Trim();

        if (normalizedCode.Length < 6)
        {
            throw new ArgumentException("MESC code must contain at least 6 digits.", nameof(code));
        }

        if (!normalizedCode[..6].All(char.IsDigit))
        {
            throw new ArgumentException("The first 6 characters of a MESC code must be digits.", nameof(code));
        }

        if (!allowNonNumeric && !normalizedCode.All(char.IsDigit))
        {
            throw new ArgumentException("MESC code must contain only digits.", nameof(code));
        }

        return normalizedCode;
    }

    private static string ValidateRequired(string value, string message, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(message, parameterName)
            : value.Trim();
}
