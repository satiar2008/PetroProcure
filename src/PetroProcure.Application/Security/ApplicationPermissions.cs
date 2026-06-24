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
    public const string TenderEdit = "Tender.Edit";
    public const string TenderPublish = "Tender.Publish";
    public const string TenderCancel = "Tender.Cancel";
    public const string TenderManageItems = "Tender.ManageItems";
    public const string TenderManageParticipants = "Tender.ManageParticipants";
    public const string TenderReceiveBid = "Tender.ReceiveBid";
    public const string TenderEvaluateQualification = "Tender.EvaluateQualification";
    public const string TenderEvaluateTechnical = "Tender.EvaluateTechnical";
    public const string TenderEvaluateCommercial = "Tender.EvaluateCommercial";
    public const string TenderCompareBids = "Tender.CompareBids";
    public const string TenderSelectWinner = "Tender.SelectWinner";
    public const string TenderClose = "Tender.Close";
    public const string TenderManageDocuments = "Tender.ManageDocuments";
    public const string TenderReportView = "Tender.ReportView";
    public const string TenderReportExport = "Tender.ReportExport";
    public const string TenderEvaluate = "Tender.Evaluate";
    public const string TenderApproveWinner = "Tender.ApproveWinner";

    public const string CommissionView = "Commission.View";
    public const string CommissionCreate = "Commission.Create";
    public const string CommissionEdit = "Commission.Edit";
    public const string CommissionSchedule = "Commission.Schedule";
    public const string CommissionStart = "Commission.Start";
    public const string CommissionComplete = "Commission.Complete";
    public const string CommissionApprove = "Commission.Approve";
    public const string CommissionCancel = "Commission.Cancel";
    public const string CommissionManageMembers = "Commission.ManageMembers";
    public const string CommissionManageAgenda = "Commission.ManageAgenda";
    public const string CommissionManageMinutes = "Commission.ManageMinutes";
    public const string CommissionManageDecisions = "Commission.ManageDecisions";
    public const string CommissionManageDocuments = "Commission.ManageDocuments";
    public const string CommissionReportView = "Commission.ReportView";
    public const string CommissionReportExport = "Commission.ReportExport";

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
        TenderEdit,
        TenderPublish,
        TenderCancel,
        TenderManageItems,
        TenderManageParticipants,
        TenderReceiveBid,
        TenderEvaluateQualification,
        TenderEvaluateTechnical,
        TenderEvaluateCommercial,
        TenderCompareBids,
        TenderSelectWinner,
        TenderClose,
        TenderManageDocuments,
        TenderReportView,
        TenderReportExport,
        TenderEvaluate,
        TenderApproveWinner,
        CommissionView,
        CommissionCreate,
        CommissionEdit,
        CommissionSchedule,
        CommissionStart,
        CommissionComplete,
        CommissionApprove,
        CommissionCancel,
        CommissionManageMembers,
        CommissionManageAgenda,
        CommissionManageMinutes,
        CommissionManageDecisions,
        CommissionManageDocuments,
        CommissionReportView,
        CommissionReportExport,
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
