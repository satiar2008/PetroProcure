using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class SupplierUiPolicy
{
    public const string View = "Supplier.View";
    public const string Create = "Supplier.Create";
    public const string Edit = "Supplier.Edit";
    public const string ActivateDeactivate = "Supplier.ActivateDeactivate";
    public const string Blacklist = "Supplier.Blacklist";
    public const string ManageContacts = "Supplier.ManageContacts";
    public const string ManageCategories = "Supplier.ManageCategories";
    public const string Evaluate = "Supplier.Evaluate";

    public static string StatusText(SupplierStatus status) => status switch
    {
        SupplierStatus.Draft => "پیش‌نویس",
        SupplierStatus.Active => "فعال",
        SupplierStatus.Inactive => "غیرفعال",
        SupplierStatus.Suspended => "تعلیق‌شده",
        SupplierStatus.Blacklisted => "فهرست سیاه",
        _ => status.ToString()
    };

    public static string TypeText(SupplierType type) => type switch
    {
        SupplierType.Manufacturer => "سازنده",
        SupplierType.Distributor => "توزیع‌کننده",
        SupplierType.Contractor => "پیمانکار",
        SupplierType.ServiceProvider => "خدمات‌دهنده",
        SupplierType.Other => "سایر",
        _ => type.ToString()
    };

    public static string EvaluationText(SupplierEvaluationResult result) => result switch
    {
        SupplierEvaluationResult.Approved => "تأییدشده",
        SupplierEvaluationResult.Conditional => "مشروط",
        SupplierEvaluationResult.Rejected => "ردشده",
        SupplierEvaluationResult.NeedsReview => "نیازمند بررسی",
        _ => result.ToString()
    };

    public static bool IsSelectable(SupplierLookupDto supplier, bool includeInactive = false, bool includeBlacklisted = false) =>
        (includeInactive || supplier.IsActive) && (includeBlacklisted || !supplier.IsBlacklisted);
}
