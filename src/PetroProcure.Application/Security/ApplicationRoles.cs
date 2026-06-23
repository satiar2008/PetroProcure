namespace PetroProcure.Application.Security;

public static class ApplicationRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string PurchaseManager = "PurchaseManager";
    public const string PurchaseExpert = "PurchaseExpert";
    public const string OrdersManager = "OrdersManager";
    public const string OrdersUser = "OrdersUser";
    public const string WarehouseManager = "WarehouseManager";
    public const string WarehouseUser = "WarehouseUser";
    public const string ApplicantUser = "ApplicantUser";
    public const string TenderCommissionManager = "TenderCommissionManager";
    public const string TenderCommissionMember = "TenderCommissionMember";
    public const string ReportViewer = "ReportViewer";
    public const string AiAgentUser = "AiAgentUser";

    public static readonly string[] All =
    [
        SystemAdmin,
        PurchaseManager,
        PurchaseExpert,
        OrdersManager,
        OrdersUser,
        WarehouseManager,
        WarehouseUser,
        ApplicantUser,
        TenderCommissionManager,
        TenderCommissionMember,
        ReportViewer,
        AiAgentUser
    ];
}
