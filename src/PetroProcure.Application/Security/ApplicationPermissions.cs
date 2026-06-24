namespace PetroProcure.Application.Security;

public static class ApplicationPermissions
{
    public const string ClaimType = "permission";

    public const string PurchaseFileCreate = "PurchaseFile.Create";
    public const string PurchaseFileView = "PurchaseFile.View";
    public const string PurchaseFileEdit = "PurchaseFile.Edit";
    public const string PurchaseFileSendToDepartment = "PurchaseFile.SendToDepartment";
    public const string PurchaseFileClose = "PurchaseFile.Close";
    public const string PurchaseFileArchive = "PurchaseFile.Archive";

    public const string IndentCreate = "Indent.Create";
    public const string IndentView = "Indent.View";
    public const string IndentApprove = "Indent.Approve";
    public const string IndentSendToPurchase = "Indent.SendToPurchase";

    public const string ItemCreate = "Item.Create";
    public const string ItemView = "Item.View";
    public const string ItemEdit = "Item.Edit";
    public const string ItemActivateDeactivate = "Item.ActivateDeactivate";

    public const string WarehouseView = "Warehouse.View";
    public const string WarehouseReceive = "Warehouse.Receive";
    public const string WarehouseIssue = "Warehouse.Issue";

    public const string TenderCreate = "Tender.Create";
    public const string TenderView = "Tender.View";
    public const string TenderEvaluate = "Tender.Evaluate";
    public const string TenderApproveWinner = "Tender.ApproveWinner";

    public const string SupplierView = "Supplier.View";
    public const string SupplierCreate = "Supplier.Create";
    public const string SupplierEdit = "Supplier.Edit";
    public const string SupplierActivateDeactivate = "Supplier.ActivateDeactivate";
    public const string SupplierBlacklist = "Supplier.Blacklist";
    public const string SupplierManageContacts = "Supplier.ManageContacts";
    public const string SupplierManageCategories = "Supplier.ManageCategories";
    public const string SupplierEvaluate = "Supplier.Evaluate";
    public const string SupplierManageDocuments = "Supplier.ManageDocuments";

    public const string InquiryView = "Inquiry.View";
    public const string InquiryCreate = "Inquiry.Create";
    public const string InquiryEdit = "Inquiry.Edit";
    public const string InquirySend = "Inquiry.Send";
    public const string InquiryCancel = "Inquiry.Cancel";
    public const string InquiryManageSuppliers = "Inquiry.ManageSuppliers";
    public const string InquiryReceiveQuote = "Inquiry.ReceiveQuote";
    public const string InquiryCompareQuotes = "Inquiry.CompareQuotes";
    public const string InquirySelectSupplier = "Inquiry.SelectSupplier";
    public const string InquiryManageDocuments = "Inquiry.ManageDocuments";

    public const string OrdersViewDashboard = "Orders.ViewDashboard";
    public const string OrdersViewInventory = "Orders.ViewInventory";
    public const string OrdersManageInventoryControl = "Orders.ManageInventoryControl";
    public const string OrdersCreateMaterialNeed = "Orders.CreateMaterialNeed";
    public const string OrdersReviewMaterialNeed = "Orders.ReviewMaterialNeed";
    public const string OrdersApproveMaterialNeed = "Orders.ApproveMaterialNeed";
    public const string OrdersConvertNeedToIndent = "Orders.ConvertNeedToIndent";
    public const string OrdersConvertShortageToIndent = "Orders.ConvertShortageToIndent";
    public const string OrdersManageShortageAlerts = "Orders.ManageShortageAlerts";

    public const string ReportView = "Report.View";
    public const string ReportPrint = "Report.Print";
    public const string ReportExportPdf = "Report.ExportPdf";

    public const string AiAgentUse = "AiAgent.Use";
    public const string AiAgentEvaluatePurchaseRules = "AiAgent.EvaluatePurchaseRules";

    public const string AdminManageUsers = "Admin.ManageUsers";
    public const string AdminManageRoles = "Admin.ManageRoles";
    public const string AdminManageDepartments = "Admin.ManageDepartments";
    public const string AdminManageSettings = "Admin.ManageSettings";

    public static readonly string[] All =
    [
        PurchaseFileCreate,
        PurchaseFileView,
        PurchaseFileEdit,
        PurchaseFileSendToDepartment,
        PurchaseFileClose,
        PurchaseFileArchive,
        IndentCreate,
        IndentView,
        IndentApprove,
        IndentSendToPurchase,
        ItemCreate,
        ItemView,
        ItemEdit,
        ItemActivateDeactivate,
        WarehouseView,
        WarehouseReceive,
        WarehouseIssue,
        TenderCreate,
        TenderView,
        TenderEvaluate,
        TenderApproveWinner,
        SupplierView,
        SupplierCreate,
        SupplierEdit,
        SupplierActivateDeactivate,
        SupplierBlacklist,
        SupplierManageContacts,
        SupplierManageCategories,
        SupplierEvaluate,
        SupplierManageDocuments,
        InquiryView,
        InquiryCreate,
        InquiryEdit,
        InquirySend,
        InquiryCancel,
        InquiryManageSuppliers,
        InquiryReceiveQuote,
        InquiryCompareQuotes,
        InquirySelectSupplier,
        InquiryManageDocuments,
        OrdersViewDashboard,
        OrdersViewInventory,
        OrdersManageInventoryControl,
        OrdersCreateMaterialNeed,
        OrdersReviewMaterialNeed,
        OrdersApproveMaterialNeed,
        OrdersConvertNeedToIndent,
        OrdersConvertShortageToIndent,
        OrdersManageShortageAlerts,
        ReportView,
        ReportPrint,
        ReportExportPdf,
        AiAgentUse,
        AiAgentEvaluatePurchaseRules,
        AdminManageUsers,
        AdminManageRoles,
        AdminManageDepartments,
        AdminManageSettings
    ];
}
