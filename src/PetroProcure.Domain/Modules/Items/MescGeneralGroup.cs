using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Items;

public sealed class MescGeneralGroup : AuditableEntity<Guid>
{
    public MescGeneralGroup(Guid id, string code, string description)
        : base(id)
    {
        Code = ValidateCode(code);
        Description = ValidateDescription(description);
        IsActive = true;
    }

    public string Code { get; private set; }

    public string Description { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(string code, string description)
    {
        Code = ValidateCode(code);
        Description = ValidateDescription(description);
        MarkModified(null);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MESC general group code is required.", nameof(code));
        }

        var normalizedCode = code.Trim();

        if (normalizedCode.Length != 6 || !normalizedCode.All(char.IsDigit))
        {
            throw new ArgumentException("MESC general group code must be exactly 6 digits.", nameof(code));
        }

        return normalizedCode;
    }

    private static string ValidateDescription(string description) =>
        string.IsNullOrWhiteSpace(description)
            ? throw new ArgumentException("General description is required.", nameof(description))
            : description.Trim();
}
