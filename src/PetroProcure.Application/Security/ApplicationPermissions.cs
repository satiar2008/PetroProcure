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
