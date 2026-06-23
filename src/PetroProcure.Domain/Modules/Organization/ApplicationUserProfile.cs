using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Organization;

public sealed class ApplicationUserProfile : AuditableEntity<Guid>
{
    public ApplicationUserProfile(Guid id, string displayName, string? email = null)
        : base(id)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Display name is required.", nameof(displayName))
            : displayName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        IsActive = true;
    }

    public string DisplayName { get; private set; }

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    public void Rename(string displayName)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Display name is required.", nameof(displayName))
            : displayName.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
