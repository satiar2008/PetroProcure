using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class Supplier : AuditableEntity<Guid>
{
    private readonly List<SupplierContact> _contacts = [];
    private readonly List<SupplierCategoryAssignment> _categoryAssignments = [];
    private readonly List<SupplierDocument> _documents = [];
    private readonly List<SupplierEvaluation> _evaluations = [];

    private Supplier()
        : base(Guid.Empty)
    {
        SupplierCode = string.Empty;
        Name = string.Empty;
    }

    public Supplier(
        Guid id,
        string supplierCode,
        string name,
        SupplierType supplierType,
        Guid createdByUserId,
        DateTime? createdAt = null)
        : base(id)
    {
        SupplierCode = Required(supplierCode, nameof(supplierCode));
        Name = Required(name, nameof(name));
        SupplierType = supplierType;
        Status = SupplierStatus.Draft;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public string SupplierCode { get; private set; }
    public string Name { get; private set; }
    public string? NationalId { get; private set; }
    public string? EconomicCode { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public SupplierStatus Status { get; private set; }
    public SupplierType SupplierType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsBlacklisted { get; private set; }
    public string? BlacklistReason { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public IReadOnlyCollection<SupplierContact> Contacts => _contacts.AsReadOnly();
    public IReadOnlyCollection<SupplierCategoryAssignment> CategoryAssignments => _categoryAssignments.AsReadOnly();
    public IReadOnlyCollection<SupplierDocument> Documents => _documents.AsReadOnly();
    public IReadOnlyCollection<SupplierEvaluation> Evaluations => _evaluations.AsReadOnly();

    public void Update(
        string supplierCode,
        string name,
        string? nationalId,
        string? economicCode,
        string? registrationNumber,
        string? phone,
        string? email,
        string? website,
        string? address,
        string? city,
        string? country,
        string? postalCode,
        SupplierType supplierType,
        string? description,
        Guid updatedByUserId)
    {
        SupplierCode = Required(supplierCode, nameof(supplierCode));
        Name = Required(name, nameof(name));
        NationalId = Trim(nationalId);
        EconomicCode = Trim(economicCode);
        RegistrationNumber = Trim(registrationNumber);
        Phone = Trim(phone);
        Email = Trim(email);
        Website = Trim(website);
        Address = Trim(address);
        City = Trim(city);
        Country = Trim(country);
        PostalCode = Trim(postalCode);
        SupplierType = supplierType;
        Description = Trim(description);
        Touch(updatedByUserId);
    }

    public void Activate(Guid userId)
    {
        IsActive = true;
        if (!IsBlacklisted)
        {
            Status = SupplierStatus.Active;
        }
        Touch(userId);
    }

    public void Deactivate(Guid userId)
    {
        IsActive = false;
        if (!IsBlacklisted)
        {
            Status = SupplierStatus.Inactive;
        }
        Touch(userId);
    }

    public void Blacklist(string reason, Guid userId)
    {
        IsBlacklisted = true;
        IsActive = false;
        Status = SupplierStatus.Blacklisted;
        BlacklistReason = Required(reason, nameof(reason));
        Touch(userId);
    }

    public void RemoveFromBlacklist(Guid userId)
    {
        IsBlacklisted = false;
        BlacklistReason = null;
        Status = IsActive ? SupplierStatus.Active : SupplierStatus.Inactive;
        Touch(userId);
    }

    public void AddContact(SupplierContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        if (contact.SupplierId != Id) throw new InvalidOperationException("Contact belongs to another supplier.");
        if (contact.IsPrimary && contact.IsActive)
        {
            foreach (var existing in _contacts.Where(x => x.IsPrimary && x.IsActive))
            {
                existing.SetPrimary(false);
            }
        }
        _contacts.Add(contact);
    }

    public void AddCategoryAssignment(SupplierCategoryAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        if (assignment.SupplierId != Id) throw new InvalidOperationException("Category assignment belongs to another supplier.");
        if (_categoryAssignments.Any(x => x.SupplierCategoryId == assignment.SupplierCategoryId))
            throw new InvalidOperationException("Supplier category is already assigned.");
        _categoryAssignments.Add(assignment);
    }

    public void AddEvaluation(SupplierEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        if (evaluation.SupplierId != Id) throw new InvalidOperationException("Evaluation belongs to another supplier.");
        _evaluations.Add(evaluation);
    }

    public bool IsEligibleForSelection(bool includeInactive = false, bool includeBlacklisted = false) =>
        (includeInactive || IsActive) && (includeBlacklisted || !IsBlacklisted);

    private void Touch(Guid userId)
    {
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
