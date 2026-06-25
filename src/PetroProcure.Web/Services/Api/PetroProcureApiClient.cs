using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Contracts.V1.Documents;
using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Contracts.V1.Mesc;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Workflow;
using PetroProcure.Contracts.V1.Identity;
using PetroProcure.Contracts.V1.Organization;
using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Contracts.V1.Inquiry;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Reports;
using PetroProcure.Contracts.V1.Contracts;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.Web.Services.Api;

public interface IPetroProcureApiClient
{
    Task<PagedResult<PurchaseFileSummaryDto>> GetPurchaseFilesAsync(PurchaseFileListRequest request, CancellationToken ct = default);
    Task<PurchaseFileDto?> GetPurchaseFileAsync(Guid id, CancellationToken ct = default);
    Task<List<PurchaseFileGroupedItemsDto>> GetGroupedItemsAsync(Guid id, CancellationToken ct = default);
    Task<List<MescItemDto>> SearchMescItemsAsync(string term, CancellationToken ct = default);
    Task<IndentDto?> GetIndentAsync(Guid id, CancellationToken ct = default);
    Task<List<IndentSummaryDto>> GetIndentsAsync(CancellationToken ct = default);
    Task<List<IndentGroupedItemsDto>> GetIndentGroupedItemsAsync(Guid id, CancellationToken ct = default);
    Task<IndentDto> CreateIndentAsync(CreateIndentRequest request, CancellationToken ct = default);
    Task<IndentItemDto> AddIndentItemAsync(Guid id, AddIndentItemRequest request, CancellationToken ct = default);
    Task RemoveIndentItemAsync(Guid id, Guid itemId, CancellationToken ct = default);
    Task ChangeIndentStatusAsync(Guid id, string action, CancellationToken ct = default);
    Task<byte[]> GetIndentPdfAsync(Guid id, CancellationToken ct = default);
    Task<List<FileDocumentDto>> GetDocumentsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<PurchaseFileDto> CreatePurchaseFileAsync(CreatePurchaseFileRequest request, CancellationToken ct = default);
    Task AddPurchaseFileItemAsync(Guid fileId, AddPurchaseFileItemRequest request, CancellationToken ct = default);
    Task<PurchaseFileDto> CreateFromIndentAsync(Guid indentId, CreatePurchaseFileFromIndentRequest request, CancellationToken ct = default);
    Task<byte[]> GetPurchaseFileSummaryPdfAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task SavePurchaseFileSummaryAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<AiEvaluationResultDto> RunAiAsync(Guid purchaseFileId, string action, CancellationToken ct = default);
    Task<List<AiEvaluationResultDto>> GetAiEvaluationsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<PurchaseFileNoteDto> AddNoteAsync(Guid purchaseFileId, AddPurchaseFileNoteRequest request, CancellationToken ct = default);
    Task RemovePurchaseFileItemAsync(Guid purchaseFileId, Guid itemId, CancellationToken ct = default);
    Task<PurchaseFileWorkflowStateDto> GetWorkflowStateAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task SendToDepartmentAsync(SendToDepartmentRequest request, CancellationToken ct = default);
    Task ReturnWorkflowAsync(ReturnWorkflowRequest request, CancellationToken ct = default);
    Task CompleteInboxTaskAsync(Guid taskId, CancellationToken ct = default);
    Task<List<InboxTaskDto>> GetMyInboxTasksAsync(CancellationToken ct = default);
    Task<List<InboxTaskDto>> GetDepartmentInboxTasksAsync(Guid departmentId, CancellationToken ct = default);
    Task<InboxTaskDetailDto> GetInboxTaskAsync(Guid taskId, CancellationToken ct = default);
    Task AssignInboxTaskToSelfAsync(Guid taskId, CancellationToken ct = default);
    Task AssignInboxTaskAsync(Guid taskId, Guid? userId, CancellationToken ct = default);
    Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task StartWorkflowAsync(StartWorkflowRequest request, CancellationToken ct = default);
    Task<DocumentUploadLimitsDto> GetUploadLimitsAsync(CancellationToken ct = default);
    Task<FileDocumentDto> UploadDocumentAsync(Guid purchaseFileId, Stream stream, string fileName,
        string contentType, UploadDocumentRequest metadata, CancellationToken ct = default);
    Task<FileDocumentDto> UploadDocumentVersionAsync(Guid documentId, Stream stream, string fileName,
        string contentType, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid documentId, CancellationToken ct = default);
    string GetDocumentDownloadUrl(Guid documentId);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<List<WorkflowActionDto>> GetAllowedWorkflowActionsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<WorkflowActionExecutionResultDto> ExecuteWorkflowActionAsync(
        Guid purchaseFileId, ExecuteWorkflowActionRequest request, CancellationToken ct = default);
    Task<List<MescGeneralGroupDto>> GetMescGroupsAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<List<MescItemDto>> GetMescItemsAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<List<MescItemGroupedDto>> GetMescItemsGroupedAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<MescGeneralGroupDto> CreateMescGroupAsync(CreateMescGeneralGroupRequest request, CancellationToken ct = default);
    Task<MescGeneralGroupDto> UpdateMescGroupAsync(Guid id, UpdateMescGeneralGroupRequest request, CancellationToken ct = default);
    Task<MescItemDto> CreateMescItemAsync(CreateMescItemRequest request, CancellationToken ct = default);
    Task<MescItemDto> UpdateMescItemAsync(Guid id, UpdateMescItemRequest request, CancellationToken ct = default);
    Task SetMescItemActiveAsync(Guid id, bool active, CancellationToken ct = default);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<List<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default);
    Task UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default);
    Task<List<PermissionDto>> GetPermissionsAsync(CancellationToken ct = default);
    Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken ct = default);
    Task AssignUserRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken ct = default);
    Task AssignUserDepartmentAsync(Guid id, AssignUserDepartmentRequest request, CancellationToken ct = default);
    Task RemoveUserDepartmentAsync(Guid id, Guid assignmentId, CancellationToken ct = default);
    Task AssignRolePermissionsAsync(Guid roleId, AssignRolePermissionsRequest request, CancellationToken ct = default);
    Task<List<WorkflowActionDefinitionDto>> GetWorkflowActionDefinitionsAsync(CancellationToken ct = default);
    Task UpdateWorkflowActionDefinitionAsync(Guid id, UpdateWorkflowActionDefinitionRequest request, CancellationToken ct = default);
    Task SetWorkflowActionDefinitionActiveAsync(Guid id, bool active, CancellationToken ct = default);
    Task<List<LookupDto>> GetUserLookupsAsync(CancellationToken ct = default);
    Task<List<LookupDto>> GetDepartmentLookupsAsync(CancellationToken ct = default);
    Task<List<AdminAuditLogDto>> GetAdminAuditLogsAsync(CancellationToken ct = default);
    Task<List<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken ct = default);
    Task<SystemSettingDto> UpdateSystemSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken ct = default);
    Task<PagedResult<PurchaseContractSummaryDto>> GetContractsAsync(ContractListRequest request, CancellationToken ct = default);
    Task<PurchaseContractDetailDto?> GetContractAsync(Guid id, CancellationToken ct = default);
    Task<List<PurchaseContractSummaryDto>> GetPurchaseFileContractsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<PurchaseContractDetailDto> CreateContractAsync(CreateContractRequest request, CancellationToken ct = default);
    Task<PurchaseContractDetailDto> CreateContractFromPurchaseFileAsync(Guid purchaseFileId, CreateContractFromPurchaseFileRequest request, CancellationToken ct = default);
    Task<PurchaseContractDetailDto> CreateContractFromTenderAsync(Guid tenderId, CreateContractFromTenderRequest request, CancellationToken ct = default);
    Task<PurchaseContractDetailDto> CreateContractFromTenderBidAsync(Guid tenderBidId, CreateContractFromTenderBidRequest request, CancellationToken ct = default);
    Task<PurchaseContractDetailDto> UpdateContractAsync(Guid id, UpdateContractRequest request, CancellationToken ct = default);
    Task<PurchaseContractItemDto> AddContractItemAsync(Guid contractId, AddContractItemRequest request, CancellationToken ct = default);
    Task RemoveContractItemAsync(Guid contractId, Guid itemId, CancellationToken ct = default);
    Task ChangeContractStatusAsync(Guid id, string action, object body, CancellationToken ct = default);
    Task<List<ContractTemplateDto>> GetContractTemplatesAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<ContractTemplateDto> CreateContractTemplateAsync(CreateContractTemplateRequest request, CancellationToken ct = default);
    Task UpdateContractTemplateAsync(Guid id, UpdateContractTemplateRequest request, CancellationToken ct = default);
    Task ApplyContractTemplateAsync(Guid contractId, Guid templateId, CancellationToken ct = default);
    Task<ContractClauseDto> AddContractClauseAsync(Guid contractId, AddContractClauseRequest request, CancellationToken ct = default);
    Task<ContractClauseDto> UpdateContractClauseAsync(Guid contractId, Guid clauseId, UpdateContractClauseRequest request, CancellationToken ct = default);
    Task RemoveContractClauseAsync(Guid contractId, Guid clauseId, CancellationToken ct = default);
    Task<List<ContractDocumentDto>> GetContractDocumentsAsync(Guid contractId, CancellationToken ct = default);
    Task<ContractDocumentDto> UploadContractDocumentAsync(Guid contractId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default);
    Task DeleteContractDocumentAsync(Guid contractId, Guid documentId, CancellationToken ct = default);
    string GetContractDocumentDownloadUrl(Guid contractId, Guid documentId);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadContractDocumentAsync(Guid contractId, Guid documentId, CancellationToken ct = default);
    Task<byte[]> GetContractPdfAsync(Guid id, CancellationToken ct = default);
    Task SaveContractPdfAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SupplierSummaryDto>> GetSuppliersAsync(SupplierListRequest request, CancellationToken ct = default);
    Task<SupplierDetailDto?> GetSupplierAsync(Guid id, CancellationToken ct = default);
    Task<SupplierDetailDto> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken ct = default);
    Task<SupplierDetailDto> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default);
    Task SetSupplierActiveAsync(Guid id, bool active, CancellationToken ct = default);
    Task BlacklistSupplierAsync(Guid id, string reason, CancellationToken ct = default);
    Task RemoveSupplierFromBlacklistAsync(Guid id, CancellationToken ct = default);
    Task<List<SupplierCategoryDto>> GetSupplierCategoriesAsync(CancellationToken ct = default);
    Task<SupplierContactDto> AddSupplierContactAsync(Guid supplierId, AddSupplierContactRequest request, CancellationToken ct = default);
    Task<SupplierContactDto> UpdateSupplierContactAsync(Guid supplierId, Guid contactId, UpdateSupplierContactRequest request, CancellationToken ct = default);
    Task DeactivateSupplierContactAsync(Guid supplierId, Guid contactId, CancellationToken ct = default);
    Task AssignSupplierCategoryAsync(Guid supplierId, Guid categoryId, CancellationToken ct = default);
    Task RemoveSupplierCategoryAsync(Guid supplierId, Guid categoryId, CancellationToken ct = default);
    Task<SupplierEvaluationDto> AddSupplierEvaluationAsync(Guid supplierId, AddSupplierEvaluationRequest request, CancellationToken ct = default);
    Task<List<SupplierLookupDto>> SearchSupplierLookupAsync(string? term, bool includeInactive = false, bool includeBlacklisted = false, CancellationToken ct = default);
    Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(InquiryListRequest request, CancellationToken ct = default);
    Task<InquiryDetailDto?> GetInquiryAsync(Guid id, CancellationToken ct = default);
    Task<List<InquirySummaryDto>> GetPurchaseFileInquiriesAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<InquiryDetailDto> CreateInquiryAsync(CreateInquiryRequest request, CancellationToken ct = default);
    Task<InquiryDetailDto> CreateInquiryFromPurchaseFileAsync(Guid purchaseFileId, CreateInquiryFromPurchaseFileRequest request, CancellationToken ct = default);
    Task SendInquiryAsync(Guid id, CancellationToken ct = default);
    Task CancelInquiryAsync(Guid id, string reason, CancellationToken ct = default);
    Task<InquirySupplierDto> AddInquirySupplierAsync(Guid id, AddInquirySupplierRequest request, CancellationToken ct = default);
    Task<SupplierQuoteDto> AddSupplierQuoteAsync(Guid id, AddSupplierQuoteRequest request, CancellationToken ct = default);
    Task<InquiryComparisonDto?> GetInquiryComparisonAsync(Guid id, CancellationToken ct = default);
    Task SelectSupplierQuoteAsync(Guid inquiryId, Guid quoteId, string? reason, CancellationToken ct = default);
    Task<OrdersDashboardDto> GetOrdersDashboardAsync(CancellationToken ct = default);
    Task<PagedResult<InventoryControlItemDto>> GetInventoryControlItemsAsync(InventoryControlListRequest request, CancellationToken ct = default);
    Task<InventoryControlItemDto> UpdateInventoryControlItemAsync(Guid id, UpdateInventoryControlItemRequest request, CancellationToken ct = default);
    Task<PagedResult<MaterialNeedDto>> GetMaterialNeedsAsync(MaterialNeedListRequest request, CancellationToken ct = default);
    Task<MaterialNeedDetailDto?> GetMaterialNeedAsync(Guid id, CancellationToken ct = default);
    Task<MaterialNeedDto> CreateMaterialNeedAsync(CreateMaterialNeedRequest request, CancellationToken ct = default);
    Task ChangeMaterialNeedStatusAsync(Guid id, string action, object? body = null, CancellationToken ct = default);
    Task<Guid> ConvertMaterialNeedToIndentAsync(Guid id, ConvertMaterialNeedToIndentRequest request, CancellationToken ct = default);
    Task<PagedResult<ShortageAlertDto>> GetShortageAlertsAsync(ShortageAlertListRequest request, CancellationToken ct = default);
    Task<List<ShortageAlertDto>> DetectShortagesAsync(DetectShortageAlertsRequest request, CancellationToken ct = default);
    Task<Guid> ConvertShortageToIndentAsync(Guid id, ConvertShortageToIndentRequest request, CancellationToken ct = default);
    Task ResolveShortageAlertAsync(Guid id, ResolveShortageAlertRequest request, CancellationToken ct = default);
    Task<PagedResult<TenderSummaryDto>> GetTendersAsync(TenderListRequest request, CancellationToken ct = default);
    Task<TenderDetailDto?> GetTenderAsync(Guid id, CancellationToken ct = default);
    Task<List<TenderSummaryDto>> GetPurchaseFileTendersAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<TenderDetailDto> CreateTenderAsync(CreateTenderRequest request, CancellationToken ct = default);
    Task<TenderDetailDto> CreateTenderFromPurchaseFileAsync(Guid purchaseFileId, CreateTenderFromPurchaseFileRequest request, CancellationToken ct = default);
    Task PublishTenderAsync(Guid id, CancellationToken ct = default);
    Task CancelTenderAsync(Guid id, string reason, CancellationToken ct = default);
    Task CloseTenderAsync(Guid id, CancellationToken ct = default);
    Task<TenderParticipantDto> AddTenderParticipantAsync(Guid id, AddTenderParticipantRequest request, CancellationToken ct = default);
    Task<TenderBidDto> AddTenderBidAsync(Guid id, AddTenderBidRequest request, CancellationToken ct = default);
    Task<TenderEvaluationDto> AddTenderEvaluationAsync(Guid id, AddTenderEvaluationRequest request, CancellationToken ct = default);
    Task<TenderComparisonDto?> GetTenderComparisonAsync(Guid id, CancellationToken ct = default);
    Task SelectTenderWinnerAsync(Guid id, SelectTenderWinnerRequest request, CancellationToken ct = default);
    Task<List<TenderDocumentDto>> GetTenderDocumentsAsync(Guid tenderId, CancellationToken ct = default);
    Task<TenderDocumentDto> UploadTenderDocumentAsync(Guid tenderId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default);
    Task DeleteTenderDocumentAsync(Guid tenderId, Guid documentId, CancellationToken ct = default);
    string GetTenderDocumentDownloadUrl(Guid tenderId, Guid documentId);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadTenderDocumentAsync(Guid tenderId, Guid documentId, CancellationToken ct = default);
    Task<byte[]> GetTenderSummaryReportPdfAsync(Guid tenderId, CancellationToken ct = default);
    Task<byte[]> GetTenderComparisonReportPdfAsync(Guid tenderId, CancellationToken ct = default);
    Task<byte[]> GetTenderWinnerDecisionReportPdfAsync(Guid tenderId, CancellationToken ct = default);
    Task<GeneratedReportResultDto> SaveTenderSummaryReportAsync(Guid tenderId, CancellationToken ct = default);
    Task<GeneratedReportResultDto> SaveTenderComparisonReportAsync(Guid tenderId, CancellationToken ct = default);
    Task<GeneratedReportResultDto> SaveTenderWinnerDecisionReportAsync(Guid tenderId, CancellationToken ct = default);
    Task<PagedResult<TenderCommissionSessionSummaryDto>> GetCommissionSessionsAsync(CommissionSessionListRequest request, CancellationToken ct = default);
    Task<TenderCommissionSessionDetailDto?> GetCommissionSessionAsync(Guid id, CancellationToken ct = default);
    Task<List<TenderCommissionSessionSummaryDto>> GetTenderCommissionSessionsAsync(Guid tenderId, CancellationToken ct = default);
    Task<List<TenderCommissionSessionSummaryDto>> GetPurchaseFileCommissionSessionsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<TenderCommissionSessionDetailDto> CreateCommissionSessionAsync(CreateCommissionSessionRequest request, CancellationToken ct = default);
    Task<TenderCommissionSessionDetailDto> CreateCommissionSessionFromTenderAsync(Guid tenderId, CreateCommissionSessionFromTenderRequest request, CancellationToken ct = default);
    Task ChangeCommissionSessionStatusAsync(Guid id, string action, object? body = null, CancellationToken ct = default);
    Task<TenderCommissionMemberDto> AddCommissionMemberAsync(Guid sessionId, AddCommissionMemberRequest request, CancellationToken ct = default);
    Task<TenderCommissionMemberDto> UpdateCommissionMemberAsync(Guid sessionId, Guid memberId, UpdateCommissionMemberRequest request, CancellationToken ct = default);
    Task RemoveCommissionMemberAsync(Guid sessionId, Guid memberId, CancellationToken ct = default);
    Task<TenderCommissionAgendaItemDto> AddCommissionAgendaItemAsync(Guid sessionId, AddAgendaItemRequest request, CancellationToken ct = default);
    Task<TenderCommissionAgendaItemDto> UpdateCommissionAgendaItemAsync(Guid sessionId, Guid agendaItemId, UpdateAgendaItemRequest request, CancellationToken ct = default);
    Task<TenderCommissionMinuteDto> AddCommissionMinuteAsync(Guid sessionId, AddCommissionMinuteRequest request, CancellationToken ct = default);
    Task<TenderCommissionMinuteDto> UpdateCommissionMinuteAsync(Guid sessionId, Guid minuteId, UpdateCommissionMinuteRequest request, CancellationToken ct = default);
    Task<TenderCommissionDecisionDto> AddCommissionDecisionAsync(Guid sessionId, AddCommissionDecisionRequest request, CancellationToken ct = default);
    Task<TenderCommissionDecisionDto> ApproveCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default);
    Task<TenderCommissionDecisionDto> RejectCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default);
    Task<List<CommissionAttachmentDto>> GetCommissionAttachmentsAsync(Guid sessionId, CancellationToken ct = default);
    Task<CommissionAttachmentDto> UploadCommissionAttachmentAsync(Guid sessionId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default);
    Task DeleteCommissionAttachmentAsync(Guid sessionId, Guid attachmentId, CancellationToken ct = default);
    string GetCommissionAttachmentDownloadUrl(Guid sessionId, Guid attachmentId);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadCommissionAttachmentAsync(Guid sessionId, Guid attachmentId, CancellationToken ct = default);
    Task<byte[]> GetCommissionMinutesReportPdfAsync(Guid sessionId, CancellationToken ct = default);
    Task<byte[]> GetCommissionDecisionReportPdfAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default);
    Task<GeneratedReportResultDto> SaveCommissionMinutesReportAsync(Guid sessionId, CancellationToken ct = default);
    Task<GeneratedReportResultDto> SaveCommissionDecisionReportAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default);
}

public sealed class PetroProcureApiClient(
    HttpClient http,
    AuthSession? session = null,
    IAuthService? auth = null) : IPetroProcureApiClient
{
    public async Task<PagedResult<PurchaseFileSummaryDto>> GetPurchaseFilesAsync(PurchaseFileListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["FileNumber"] = r.FileNumber,
            ["Title"] = r.Title,
            ["Status"] = r.Status?.ToString(),
            ["CurrentDepartmentId"] = r.CurrentDepartmentId?.ToString(),
            ["CreatedDateFrom"] = r.CreatedDateFrom?.ToString("O"),
            ["CreatedDateTo"] = r.CreatedDateTo?.ToString("O"),
            ["SourceIndentNumber"] = r.SourceIndentNumber,
            ["SortBy"] = r.SortBy,
            ["SortDescending"] = r.SortDescending.ToString(),
            ["PageNumber"] = r.PageNumber.ToString(),
            ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/purchase-files?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<PurchaseFileSummaryDto>>(url, ct)
            ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<PurchaseFileDto?> GetPurchaseFileAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<PurchaseFileDto>($"/api/purchase-files/{id}", ct);

    public async Task<List<PurchaseFileGroupedItemsDto>> GetGroupedItemsAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<List<PurchaseFileGroupedItemsDto>>($"/api/purchase-files/{id}/items/grouped", ct) ?? [];

    public async Task<List<MescItemDto>> SearchMescItemsAsync(string term, CancellationToken ct = default) =>
        await GetJsonAsync<List<MescItemDto>>(
            $"/api/mesc/items/search?term={Uri.EscapeDataString(term)}&includeInactive=false", ct) ?? [];

    public Task<IndentDto?> GetIndentAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<IndentDto>($"/api/indents/{id}", ct);
    public async Task<List<IndentSummaryDto>> GetIndentsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<IndentSummaryDto>>("/api/indents", ct) ?? [];
    public async Task<List<IndentGroupedItemsDto>> GetIndentGroupedItemsAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<List<IndentGroupedItemsDto>>($"/api/indents/{id}/items/grouped", ct) ?? [];
    public async Task<IndentDto> CreateIndentAsync(CreateIndentRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/indents", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<IndentDto>(cancellationToken: ct))!;
    }
    public async Task<IndentItemDto> AddIndentItemAsync(Guid id, AddIndentItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/indents/{id}/items", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<IndentItemDto>(cancellationToken: ct))!;
    }
    public async Task RemoveIndentItemAsync(Guid id, Guid itemId, CancellationToken ct = default)
    { var response = await Client().DeleteAsync($"/api/indents/{id}/items/{itemId}", ct); await Ensure(response, ct); }
    public async Task ChangeIndentStatusAsync(Guid id, string action, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/indents/{id}/{action}", null, ct); await Ensure(response, ct); }
    public Task<byte[]> GetIndentPdfAsync(Guid id, CancellationToken ct = default) =>
        GetBytesAsync($"/api/reports/indent/{id}/pdf", ct);

    public async Task<List<FileDocumentDto>> GetDocumentsAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<List<FileDocumentDto>>(
            $"/api/purchase-files/{id}/documents?includeDeleted=false", ct) ?? [];

    public async Task<PurchaseFileDto> CreatePurchaseFileAsync(CreatePurchaseFileRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/purchase-files", request, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseFileDto>(cancellationToken: ct))!;
    }

    public async Task AddPurchaseFileItemAsync(Guid id, AddPurchaseFileItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/purchase-files/{id}/items", request, ct);
        await Ensure(response, ct);
    }

    public async Task<PurchaseFileDto> CreateFromIndentAsync(
        Guid id, CreatePurchaseFileFromIndentRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/purchase-files/from-indent/{id}", request, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseFileDto>(cancellationToken: ct))!;
    }

    public Task<byte[]> GetPurchaseFileSummaryPdfAsync(Guid id, CancellationToken ct = default) =>
        GetBytesAsync($"/api/reports/purchase-file-summary/{id}/pdf", ct);

    public async Task SavePurchaseFileSummaryAsync(Guid id, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/reports/purchase-file-summary/{id}/save-to-file", null, ct);
        await Ensure(response, ct);
    }

    public async Task<AiEvaluationResultDto> RunAiAsync(Guid id, string action, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/ai/purchase-files/{id}/{action}", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<AiEvaluationResultDto>(cancellationToken: ct))!;
    }

    public async Task<List<AiEvaluationResultDto>> GetAiEvaluationsAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<List<AiEvaluationResultDto>>($"/api/ai/purchase-files/{id}/evaluations", ct) ?? [];
    public async Task<PurchaseFileNoteDto> AddNoteAsync(Guid id, AddPurchaseFileNoteRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/purchase-files/{id}/notes", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseFileNoteDto>(cancellationToken: ct))!;
    }
    public async Task RemovePurchaseFileItemAsync(Guid id, Guid itemId, CancellationToken ct = default)
    { var response = await Client().DeleteAsync($"/api/purchase-files/{id}/items/{itemId}", ct); await Ensure(response, ct); }
    public async Task<PurchaseFileWorkflowStateDto> GetWorkflowStateAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<PurchaseFileWorkflowStateDto>($"/api/purchase-files/{id}/workflow/state", ct)
        ?? throw new HttpRequestException("وضعیت گردش کار دریافت نشد.");
    public async Task SendToDepartmentAsync(SendToDepartmentRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync("/api/workflow/send-to-department", request, ct); await Ensure(response, ct); }
    public async Task ReturnWorkflowAsync(ReturnWorkflowRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync("/api/workflow/return", request, ct); await Ensure(response, ct); }
    public async Task CompleteInboxTaskAsync(Guid taskId, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/inbox/{taskId}/complete", null, ct); await Ensure(response, ct); }
    public async Task<List<InboxTaskDto>> GetMyInboxTasksAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<InboxTaskDto>>("/api/inbox/my", ct) ?? [];
    public async Task<List<InboxTaskDto>> GetDepartmentInboxTasksAsync(Guid departmentId, CancellationToken ct = default) =>
        await GetJsonAsync<List<InboxTaskDto>>($"/api/inbox/department/{departmentId}", ct) ?? [];
    public async Task<InboxTaskDetailDto> GetInboxTaskAsync(Guid taskId, CancellationToken ct = default) =>
        await GetJsonAsync<InboxTaskDetailDto>($"/api/inbox/tasks/{taskId}", ct)
        ?? throw new HttpRequestException("جزئیات کارتابل دریافت نشد.");
    public async Task AssignInboxTaskToSelfAsync(Guid taskId, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/inbox/{taskId}/assign-to-self", null, ct); await Ensure(response, ct); }
    public async Task AssignInboxTaskAsync(Guid taskId, Guid? userId, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inbox/{taskId}/assign", new AssignInboxTaskRequest(userId), ct); await Ensure(response, ct); }
    public async Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<UserDto>>("/api/users", ct) ?? [];
    public async Task StartWorkflowAsync(StartWorkflowRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync("/api/workflow/start", request, ct); await Ensure(response, ct); }
    public async Task<DocumentUploadLimitsDto> GetUploadLimitsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<DocumentUploadLimitsDto>("/api/documents/upload-limits", ct)
        ?? throw new HttpRequestException("محدودیت‌های بارگذاری دریافت نشد.");
    public async Task<FileDocumentDto> UploadDocumentAsync(Guid id, Stream stream, string fileName,
        string contentType, UploadDocumentRequest metadata, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        form.Add(new StringContent(metadata.DocumentType.ToString()), "documentType");
        if (metadata.DepartmentId.HasValue) form.Add(new StringContent(metadata.DepartmentId.Value.ToString()), "departmentId");
        if (!string.IsNullOrWhiteSpace(metadata.Description)) form.Add(new StringContent(metadata.Description), "description");
        var response = await Client().PostAsync($"/api/purchase-files/{id}/documents/upload", form, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<FileDocumentDto>(cancellationToken: ct))!;
    }
    public async Task<FileDocumentDto> UploadDocumentVersionAsync(Guid id, Stream stream, string fileName,
        string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream); content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        var response = await Client().PostAsync($"/api/documents/{id}/versions", form, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<FileDocumentDto>(cancellationToken: ct))!;
    }
    public async Task DeleteDocumentAsync(Guid id, CancellationToken ct = default)
    { var response = await Client().DeleteAsync($"/api/documents/{id}", ct); await Ensure(response, ct); }
    public string GetDocumentDownloadUrl(Guid id) =>
        new Uri(Client().BaseAddress!, $"/api/documents/{id}/download").ToString();
    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadDocumentAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await SendGetAsync($"/api/documents/{id}/download", ct);
        await Ensure(response, ct);
        return await ReadFileResponseAsync(response, "document", ct);
    }
    public async Task<List<WorkflowActionDto>> GetAllowedWorkflowActionsAsync(Guid id, CancellationToken ct = default) =>
        await GetJsonAsync<List<WorkflowActionDto>>(
            $"/api/purchase-files/{id}/workflow/allowed-actions", ct) ?? [];
    public async Task<WorkflowActionExecutionResultDto> ExecuteWorkflowActionAsync(
        Guid id, ExecuteWorkflowActionRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync(
            $"/api/purchase-files/{id}/workflow/execute-action", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<WorkflowActionExecutionResultDto>(cancellationToken: ct))!;
    }
    public async Task<List<MescGeneralGroupDto>> GetMescGroupsAsync(bool includeInactive = false, CancellationToken ct = default) =>
        await GetJsonAsync<List<MescGeneralGroupDto>>($"/api/mesc/groups?includeInactive={includeInactive.ToString().ToLowerInvariant()}", ct) ?? [];
    public async Task<List<MescItemDto>> GetMescItemsAsync(bool includeInactive = false, CancellationToken ct = default) =>
        await GetJsonAsync<List<MescItemDto>>($"/api/mesc/items?includeInactive={includeInactive.ToString().ToLowerInvariant()}", ct) ?? [];
    public async Task<List<MescItemGroupedDto>> GetMescItemsGroupedAsync(bool includeInactive = false, CancellationToken ct = default) =>
        await GetJsonAsync<List<MescItemGroupedDto>>($"/api/mesc/items/grouped?includeInactive={includeInactive.ToString().ToLowerInvariant()}", ct) ?? [];
    public async Task<MescGeneralGroupDto> CreateMescGroupAsync(CreateMescGeneralGroupRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/mesc/groups", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<MescGeneralGroupDto>(cancellationToken: ct))!;
    }
    public async Task<MescGeneralGroupDto> UpdateMescGroupAsync(Guid id, UpdateMescGeneralGroupRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/mesc/groups/{id}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<MescGeneralGroupDto>(cancellationToken: ct))!;
    }
    public async Task<MescItemDto> CreateMescItemAsync(CreateMescItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/mesc/items", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<MescItemDto>(cancellationToken: ct))!;
    }
    public async Task<MescItemDto> UpdateMescItemAsync(Guid id, UpdateMescItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/mesc/items/{id}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<MescItemDto>(cancellationToken: ct))!;
    }
    public async Task SetMescItemActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/mesc/items/{id}/{(active ? "activate" : "deactivate")}", null, ct);
        await Ensure(response, ct);
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default) =>
        await GetJsonAsync<AdminDashboardDto>("/api/admin/dashboard", ct)
        ?? new(0, 0, 0, 0, 0);
    public async Task<List<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<DepartmentDto>>("/api/departments", ct) ?? [];
    public async Task UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default)
    { var response = await Client().PutAsJsonAsync($"/api/departments/{id}", request, ct); await Ensure(response, ct); }
    public async Task<List<PermissionDto>> GetPermissionsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<PermissionDto>>("/api/permissions", ct) ?? [];
    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<RoleDto>>("/api/roles", ct) ?? [];
    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/roles", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<RoleDto>(cancellationToken: ct))!;
    }
    public async Task UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken ct = default)
    { var response = await Client().PutAsJsonAsync($"/api/roles/{roleId}", request, ct); await Ensure(response, ct); }
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/users", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct))!;
    }
    public async Task UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    { var response = await Client().PutAsJsonAsync($"/api/users/{id}", request, ct); await Ensure(response, ct); }
    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/users/{id}/reset-password", request, ct); await Ensure(response, ct); }
    public async Task AssignUserRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/users/{id}/roles", request, ct); await Ensure(response, ct); }
    public async Task AssignUserDepartmentAsync(Guid id, AssignUserDepartmentRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/users/{id}/departments", request, ct); await Ensure(response, ct); }
    public async Task RemoveUserDepartmentAsync(Guid id, Guid assignmentId, CancellationToken ct = default)
    { var response = await Client().DeleteAsync($"/api/users/{id}/departments/{assignmentId}", ct); await Ensure(response, ct); }
    public async Task AssignRolePermissionsAsync(Guid roleId, AssignRolePermissionsRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/roles/{roleId}/permissions", request, ct); await Ensure(response, ct); }
    public async Task<List<WorkflowActionDefinitionDto>> GetWorkflowActionDefinitionsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<WorkflowActionDefinitionDto>>("/api/workflow/action-definitions", ct) ?? [];
    public async Task UpdateWorkflowActionDefinitionAsync(Guid id, UpdateWorkflowActionDefinitionRequest request, CancellationToken ct = default)
    { var response = await Client().PutAsJsonAsync($"/api/workflow/action-definitions/{id}", request, ct); await Ensure(response, ct); }
    public async Task SetWorkflowActionDefinitionActiveAsync(Guid id, bool active, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/workflow/action-definitions/{id}/{(active ? "activate" : "deactivate")}", null, ct); await Ensure(response, ct); }
    public async Task<List<LookupDto>> GetUserLookupsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<LookupDto>>("/api/lookups/users", ct) ?? [];
    public async Task<List<LookupDto>> GetDepartmentLookupsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<LookupDto>>("/api/lookups/departments", ct) ?? [];
    public async Task<List<AdminAuditLogDto>> GetAdminAuditLogsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<AdminAuditLogDto>>("/api/admin/audit-logs", ct) ?? [];
    public async Task<List<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<SystemSettingDto>>("/api/admin/settings", ct) ?? [];
    public async Task<SystemSettingDto> UpdateSystemSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/admin/settings/{Uri.EscapeDataString(key)}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SystemSettingDto>(cancellationToken: ct))!;
    }

    public async Task<PagedResult<PurchaseContractSummaryDto>> GetContractsAsync(ContractListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["searchTerm"] = r.SearchTerm,
            ["status"] = r.Status?.ToString(),
            ["contractType"] = r.ContractType?.ToString(),
            ["supplierId"] = r.SupplierId?.ToString(),
            ["purchaseFileNumber"] = r.PurchaseFileNumber,
            ["tenderNumber"] = r.TenderNumber,
            ["createdDateFrom"] = r.CreatedDateFrom?.ToString("O"),
            ["createdDateTo"] = r.CreatedDateTo?.ToString("O"),
            ["sortBy"] = r.SortBy,
            ["sortDescending"] = r.SortDescending.ToString().ToLowerInvariant(),
            ["pageNumber"] = r.PageNumber.ToString(),
            ["pageSize"] = r.PageSize.ToString()
        };
        var url = "/api/contracts?" + string.Join("&", query
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<PurchaseContractSummaryDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<PurchaseContractDetailDto?> GetContractAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<PurchaseContractDetailDto>($"/api/contracts/{id}", ct);

    public async Task<List<PurchaseContractSummaryDto>> GetPurchaseFileContractsAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        await GetJsonAsync<List<PurchaseContractSummaryDto>>($"/api/purchase-files/{purchaseFileId}/contracts", ct) ?? [];

    public async Task<PurchaseContractDetailDto> CreateContractAsync(CreateContractRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/contracts", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractDetailDto>(cancellationToken: ct))!;
    }

    public async Task<PurchaseContractDetailDto> CreateContractFromPurchaseFileAsync(Guid purchaseFileId, CreateContractFromPurchaseFileRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/from-purchase-file/{purchaseFileId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractDetailDto>(cancellationToken: ct))!;
    }

    public async Task<PurchaseContractDetailDto> CreateContractFromTenderAsync(Guid tenderId, CreateContractFromTenderRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/from-tender/{tenderId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractDetailDto>(cancellationToken: ct))!;
    }

    public async Task<PurchaseContractDetailDto> CreateContractFromTenderBidAsync(Guid tenderBidId, CreateContractFromTenderBidRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/from-tender-bid/{tenderBidId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractDetailDto>(cancellationToken: ct))!;
    }

    public async Task<PurchaseContractDetailDto> UpdateContractAsync(Guid id, UpdateContractRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/contracts/{id}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractDetailDto>(cancellationToken: ct))!;
    }

    public async Task<PurchaseContractItemDto> AddContractItemAsync(Guid contractId, AddContractItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/{contractId}/items", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<PurchaseContractItemDto>(cancellationToken: ct))!;
    }

    public async Task RemoveContractItemAsync(Guid contractId, Guid itemId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/contracts/{contractId}/items/{itemId}", ct); await Ensure(response, ct);
    }

    public async Task ChangeContractStatusAsync(Guid id, string action, object body, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/{id}/{action}", body, ct); await Ensure(response, ct);
    }

    public async Task<List<ContractTemplateDto>> GetContractTemplatesAsync(bool includeInactive = false, CancellationToken ct = default) =>
        await GetJsonAsync<List<ContractTemplateDto>>($"/api/contracts/templates?includeInactive={includeInactive.ToString().ToLowerInvariant()}", ct) ?? [];

    public async Task<ContractTemplateDto> CreateContractTemplateAsync(CreateContractTemplateRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/contracts/templates", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<ContractTemplateDto>(cancellationToken: ct))!;
    }

    public async Task UpdateContractTemplateAsync(Guid id, UpdateContractTemplateRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/contracts/templates/{id}", request, ct); await Ensure(response, ct);
    }

    public async Task ApplyContractTemplateAsync(Guid contractId, Guid templateId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/contracts/{contractId}/apply-template/{templateId}", null, ct); await Ensure(response, ct);
    }

    public async Task<ContractClauseDto> AddContractClauseAsync(Guid contractId, AddContractClauseRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/contracts/{contractId}/clauses", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<ContractClauseDto>(cancellationToken: ct))!;
    }

    public async Task<ContractClauseDto> UpdateContractClauseAsync(Guid contractId, Guid clauseId, UpdateContractClauseRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/contracts/{contractId}/clauses/{clauseId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<ContractClauseDto>(cancellationToken: ct))!;
    }

    public async Task RemoveContractClauseAsync(Guid contractId, Guid clauseId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/contracts/{contractId}/clauses/{clauseId}", ct); await Ensure(response, ct);
    }

    public async Task<List<ContractDocumentDto>> GetContractDocumentsAsync(Guid contractId, CancellationToken ct = default) =>
        await GetJsonAsync<List<ContractDocumentDto>>($"/api/contracts/{contractId}/documents", ct) ?? [];

    public async Task<ContractDocumentDto> UploadContractDocumentAsync(Guid contractId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        form.Add(new StringContent(documentType), "documentType");
        if (!string.IsNullOrWhiteSpace(description)) form.Add(new StringContent(description), "description");
        var response = await Client().PostAsync($"/api/contracts/{contractId}/documents/upload", form, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<ContractDocumentDto>(cancellationToken: ct))!;
    }

    public async Task DeleteContractDocumentAsync(Guid contractId, Guid documentId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/contracts/{contractId}/documents/{documentId}", ct);
        await Ensure(response, ct);
    }

    public string GetContractDocumentDownloadUrl(Guid contractId, Guid documentId) =>
        $"/contract-document-download/{contractId}/{documentId}";

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadContractDocumentAsync(Guid contractId, Guid documentId, CancellationToken ct = default)
    {
        using var response = await SendGetAsync($"/api/contracts/{contractId}/documents/{documentId}/download", ct);
        await Ensure(response, ct);
        return await ReadFileResponseAsync(response, "contract-document", ct);
    }

    public Task<byte[]> GetContractPdfAsync(Guid id, CancellationToken ct = default) =>
        GetBytesAsync($"/api/contracts/{id}/reports/contract/pdf", ct);

    public async Task SaveContractPdfAsync(Guid id, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/contracts/{id}/reports/contract/save-to-file", null, ct); await Ensure(response, ct);
    }

    public async Task<PagedResult<SupplierSummaryDto>> GetSuppliersAsync(SupplierListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["SearchTerm"] = r.SearchTerm,
            ["Status"] = r.Status?.ToString(),
            ["SupplierType"] = r.SupplierType?.ToString(),
            ["CategoryId"] = r.CategoryId?.ToString(),
            ["IsActive"] = r.IsActive?.ToString(),
            ["IsBlacklisted"] = r.IsBlacklisted?.ToString(),
            ["City"] = r.City,
            ["HasPrimaryContact"] = r.HasPrimaryContact?.ToString(),
            ["SortBy"] = r.SortBy,
            ["SortDescending"] = r.SortDescending.ToString(),
            ["PageNumber"] = r.PageNumber.ToString(),
            ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/suppliers?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<SupplierSummaryDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<SupplierDetailDto?> GetSupplierAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<SupplierDetailDto>($"/api/suppliers/{id}", ct);

    public async Task<SupplierDetailDto> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/suppliers", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SupplierDetailDto>(cancellationToken: ct))!;
    }

    public async Task<SupplierDetailDto> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/suppliers/{id}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SupplierDetailDto>(cancellationToken: ct))!;
    }

    public async Task SetSupplierActiveAsync(Guid id, bool active, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/suppliers/{id}/{(active ? "activate" : "deactivate")}", null, ct); await Ensure(response, ct); }

    public async Task BlacklistSupplierAsync(Guid id, string reason, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/suppliers/{id}/blacklist", new ChangeSupplierStatusRequest(reason), ct); await Ensure(response, ct); }

    public async Task RemoveSupplierFromBlacklistAsync(Guid id, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/suppliers/{id}/remove-from-blacklist", null, ct); await Ensure(response, ct); }

    public async Task<List<SupplierCategoryDto>> GetSupplierCategoriesAsync(CancellationToken ct = default) =>
        await GetJsonAsync<List<SupplierCategoryDto>>("/api/suppliers/categories", ct) ?? [];

    public async Task<SupplierContactDto> AddSupplierContactAsync(Guid supplierId, AddSupplierContactRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/suppliers/{supplierId}/contacts", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SupplierContactDto>(cancellationToken: ct))!;
    }

    public async Task<SupplierContactDto> UpdateSupplierContactAsync(Guid supplierId, Guid contactId, UpdateSupplierContactRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/suppliers/{supplierId}/contacts/{contactId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SupplierContactDto>(cancellationToken: ct))!;
    }

    public async Task DeactivateSupplierContactAsync(Guid supplierId, Guid contactId, CancellationToken ct = default)
    { var response = await Client().PostAsync($"/api/suppliers/{supplierId}/contacts/{contactId}/deactivate", null, ct); await Ensure(response, ct); }

    public async Task AssignSupplierCategoryAsync(Guid supplierId, Guid categoryId, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/suppliers/{supplierId}/categories", new AssignSupplierCategoryRequest(categoryId), ct); await Ensure(response, ct); }

    public async Task RemoveSupplierCategoryAsync(Guid supplierId, Guid categoryId, CancellationToken ct = default)
    { var response = await Client().DeleteAsync($"/api/suppliers/{supplierId}/categories/{categoryId}", ct); await Ensure(response, ct); }

    public async Task<SupplierEvaluationDto> AddSupplierEvaluationAsync(Guid supplierId, AddSupplierEvaluationRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/suppliers/{supplierId}/evaluations", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<SupplierEvaluationDto>(cancellationToken: ct))!;
    }

    public async Task<List<SupplierLookupDto>> SearchSupplierLookupAsync(string? term, bool includeInactive = false, bool includeBlacklisted = false, CancellationToken ct = default) =>
        await GetJsonAsync<List<SupplierLookupDto>>($"/api/suppliers/lookup?term={Uri.EscapeDataString(term ?? string.Empty)}&includeInactive={includeInactive.ToString().ToLowerInvariant()}&includeBlacklisted={includeBlacklisted.ToString().ToLowerInvariant()}", ct) ?? [];

    public async Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(InquiryListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["SearchTerm"] = r.SearchTerm, ["InquiryNumber"] = r.InquiryNumber, ["PurchaseFileNumber"] = r.PurchaseFileNumber,
            ["Status"] = r.Status?.ToString(), ["InquiryType"] = r.InquiryType?.ToString(), ["SupplierId"] = r.SupplierId?.ToString(),
            ["CreatedDateFrom"] = r.CreatedDateFrom?.ToString("O"), ["CreatedDateTo"] = r.CreatedDateTo?.ToString("O"),
            ["DeadlineDateFrom"] = r.DeadlineDateFrom?.ToString("O"), ["DeadlineDateTo"] = r.DeadlineDateTo?.ToString("O"),
            ["SortBy"] = r.SortBy, ["SortDescending"] = r.SortDescending.ToString(), ["PageNumber"] = r.PageNumber.ToString(), ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/inquiries?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<InquirySummaryDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }
    public Task<InquiryDetailDto?> GetInquiryAsync(Guid id, CancellationToken ct = default) => GetJsonAsync<InquiryDetailDto>($"/api/inquiries/{id}", ct);
    public async Task<List<InquirySummaryDto>> GetPurchaseFileInquiriesAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        await GetJsonAsync<List<InquirySummaryDto>>($"/api/purchase-files/{purchaseFileId}/inquiries", ct) ?? [];
    public async Task<InquiryDetailDto> CreateInquiryAsync(CreateInquiryRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync("/api/inquiries", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<InquiryDetailDto>(cancellationToken: ct))!; }
    public async Task<InquiryDetailDto> CreateInquiryFromPurchaseFileAsync(Guid purchaseFileId, CreateInquiryFromPurchaseFileRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/from-purchase-file/{purchaseFileId}", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<InquiryDetailDto>(cancellationToken: ct))!; }
    public async Task SendInquiryAsync(Guid id, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/{id}/send", new SendInquiryRequest(), ct); await Ensure(response, ct); }
    public async Task CancelInquiryAsync(Guid id, string reason, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/{id}/cancel", new CancelInquiryRequest(reason), ct); await Ensure(response, ct); }
    public async Task<InquirySupplierDto> AddInquirySupplierAsync(Guid id, AddInquirySupplierRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/{id}/suppliers", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<InquirySupplierDto>(cancellationToken: ct))!; }
    public async Task<SupplierQuoteDto> AddSupplierQuoteAsync(Guid id, AddSupplierQuoteRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/{id}/quotes", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<SupplierQuoteDto>(cancellationToken: ct))!; }
    public Task<InquiryComparisonDto?> GetInquiryComparisonAsync(Guid id, CancellationToken ct = default) => GetJsonAsync<InquiryComparisonDto>($"/api/inquiries/{id}/comparison", ct);
    public async Task SelectSupplierQuoteAsync(Guid inquiryId, Guid quoteId, string? reason, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/inquiries/{inquiryId}/quotes/{quoteId}/select", new SelectSupplierQuoteRequest(reason), ct); await Ensure(response, ct); }

    public async Task<OrdersDashboardDto> GetOrdersDashboardAsync(CancellationToken ct = default) =>
        await GetJsonAsync<OrdersDashboardDto>("/api/orders/dashboard", ct) ?? new(0, 0, 0, 0, 0, [], []);

    public async Task<PagedResult<InventoryControlItemDto>> GetInventoryControlItemsAsync(InventoryControlListRequest r, CancellationToken ct = default)
    {
        var url = $"/api/orders/inventory-control?SearchTerm={Uri.EscapeDataString(r.SearchTerm ?? "")}&IncludeInactive={r.IncludeInactive}&PageNumber={r.PageNumber}&PageSize={r.PageSize}";
        return await GetJsonAsync<PagedResult<InventoryControlItemDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public async Task<InventoryControlItemDto> UpdateInventoryControlItemAsync(Guid id, UpdateInventoryControlItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/orders/inventory-control/{id}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<InventoryControlItemDto>(cancellationToken: ct))!;
    }

    public async Task<PagedResult<MaterialNeedDto>> GetMaterialNeedsAsync(MaterialNeedListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["Status"] = r.Status?.ToString(), ["Priority"] = r.Priority?.ToString(), ["MescCode"] = r.MescCode,
            ["ApplicantDepartmentId"] = r.ApplicantDepartmentId?.ToString(), ["CreatedDateFrom"] = r.CreatedDateFrom?.ToString("O"),
            ["CreatedDateTo"] = r.CreatedDateTo?.ToString("O"), ["PageNumber"] = r.PageNumber.ToString(), ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/orders/material-needs?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<MaterialNeedDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<MaterialNeedDetailDto?> GetMaterialNeedAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<MaterialNeedDetailDto>($"/api/orders/material-needs/{id}", ct);

    public async Task<MaterialNeedDto> CreateMaterialNeedAsync(CreateMaterialNeedRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/orders/material-needs", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<MaterialNeedDto>(cancellationToken: ct))!;
    }

    public async Task ChangeMaterialNeedStatusAsync(Guid id, string action, object? body = null, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/orders/material-needs/{id}/{action}", body ?? new { }, ct); await Ensure(response, ct);
    }

    public async Task<Guid> ConvertMaterialNeedToIndentAsync(Guid id, ConvertMaterialNeedToIndentRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/orders/material-needs/{id}/convert-to-indent", request, ct); await Ensure(response, ct);
        var payload = await response.Content.ReadFromJsonAsync<IndentReference>(cancellationToken: ct);
        return payload!.IndentId;
    }

    public async Task<PagedResult<ShortageAlertDto>> GetShortageAlertsAsync(ShortageAlertListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["Status"] = r.Status?.ToString(),
            ["MescCode"] = r.MescCode,
            ["PageNumber"] = r.PageNumber.ToString(),
            ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/orders/shortage-alerts?" + string.Join("&",
            query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<ShortageAlertDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public async Task<List<ShortageAlertDto>> DetectShortagesAsync(DetectShortageAlertsRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/orders/shortage-alerts/detect", request, ct); await Ensure(response, ct);
        return await response.Content.ReadFromJsonAsync<List<ShortageAlertDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<Guid> ConvertShortageToIndentAsync(Guid id, ConvertShortageToIndentRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/orders/shortage-alerts/{id}/convert-to-indent", request, ct); await Ensure(response, ct);
        var payload = await response.Content.ReadFromJsonAsync<IndentReference>(cancellationToken: ct);
        return payload!.IndentId;
    }

    public async Task ResolveShortageAlertAsync(Guid id, ResolveShortageAlertRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/orders/shortage-alerts/{id}/resolve", request, ct); await Ensure(response, ct);
    }

    public async Task<PagedResult<TenderSummaryDto>> GetTendersAsync(TenderListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["SearchTerm"] = r.SearchTerm,
            ["TenderNumber"] = r.TenderNumber,
            ["PurchaseFileNumber"] = r.PurchaseFileNumber,
            ["Status"] = r.Status?.ToString(),
            ["TenderType"] = r.TenderType?.ToString(),
            ["SupplierId"] = r.SupplierId?.ToString(),
            ["CreatedDateFrom"] = r.CreatedDateFrom?.ToString("O"),
            ["CreatedDateTo"] = r.CreatedDateTo?.ToString("O"),
            ["SubmissionDeadlineFrom"] = r.SubmissionDeadlineFrom?.ToString("O"),
            ["SubmissionDeadlineTo"] = r.SubmissionDeadlineTo?.ToString("O"),
            ["SortBy"] = r.SortBy,
            ["SortDescending"] = r.SortDescending.ToString(),
            ["PageNumber"] = r.PageNumber.ToString(),
            ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/tenders?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<TenderSummaryDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<TenderDetailDto?> GetTenderAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<TenderDetailDto>($"/api/tenders/{id}", ct);

    public async Task<List<TenderSummaryDto>> GetPurchaseFileTendersAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        await GetJsonAsync<List<TenderSummaryDto>>($"/api/purchase-files/{purchaseFileId}/tenders", ct) ?? [];

    public async Task<TenderDetailDto> CreateTenderAsync(CreateTenderRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/tenders", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderDetailDto>(cancellationToken: ct))!;
    }

    public async Task<TenderDetailDto> CreateTenderFromPurchaseFileAsync(Guid purchaseFileId, CreateTenderFromPurchaseFileRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/tenders/from-purchase-file/{purchaseFileId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderDetailDto>(cancellationToken: ct))!;
    }

    public async Task PublishTenderAsync(Guid id, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/publish", new PublishTenderRequest(), ct); await Ensure(response, ct); }
    public async Task CancelTenderAsync(Guid id, string reason, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/cancel", new CancelTenderRequest(reason), ct); await Ensure(response, ct); }
    public async Task CloseTenderAsync(Guid id, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/close", new CloseTenderRequest(), ct); await Ensure(response, ct); }
    public async Task<TenderParticipantDto> AddTenderParticipantAsync(Guid id, AddTenderParticipantRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/participants", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<TenderParticipantDto>(cancellationToken: ct))!; }
    public async Task<TenderBidDto> AddTenderBidAsync(Guid id, AddTenderBidRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/bids", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<TenderBidDto>(cancellationToken: ct))!; }
    public async Task<TenderEvaluationDto> AddTenderEvaluationAsync(Guid id, AddTenderEvaluationRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/evaluations", request, ct); await Ensure(response, ct); return (await response.Content.ReadFromJsonAsync<TenderEvaluationDto>(cancellationToken: ct))!; }
    public Task<TenderComparisonDto?> GetTenderComparisonAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<TenderComparisonDto>($"/api/tenders/{id}/comparison", ct);
    public async Task SelectTenderWinnerAsync(Guid id, SelectTenderWinnerRequest request, CancellationToken ct = default)
    { var response = await Client().PostAsJsonAsync($"/api/tenders/{id}/select-winner", request, ct); await Ensure(response, ct); }

    public async Task<List<TenderDocumentDto>> GetTenderDocumentsAsync(Guid tenderId, CancellationToken ct = default) =>
        await GetJsonAsync<List<TenderDocumentDto>>($"/api/tenders/{tenderId}/documents", ct) ?? [];

    public async Task<TenderDocumentDto> UploadTenderDocumentAsync(Guid tenderId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        form.Add(new StringContent(documentType), "documentType");
        if (!string.IsNullOrWhiteSpace(description)) form.Add(new StringContent(description), "description");
        var response = await Client().PostAsync($"/api/tenders/{tenderId}/documents/upload", form, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderDocumentDto>(cancellationToken: ct))!;
    }

    public async Task DeleteTenderDocumentAsync(Guid tenderId, Guid documentId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/tenders/{tenderId}/documents/{documentId}", ct);
        await Ensure(response, ct);
    }

    public string GetTenderDocumentDownloadUrl(Guid tenderId, Guid documentId) =>
        $"/tender-document-download/{tenderId}/{documentId}";

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadTenderDocumentAsync(Guid tenderId, Guid documentId, CancellationToken ct = default)
    {
        using var response = await SendGetAsync($"/api/tenders/{tenderId}/documents/{documentId}/download", ct);
        await Ensure(response, ct);
        return await ReadFileResponseAsync(response, "tender-document", ct);
    }

    public Task<byte[]> GetTenderSummaryReportPdfAsync(Guid tenderId, CancellationToken ct = default) =>
        GetBytesAsync($"/api/tenders/{tenderId}/reports/summary/pdf", ct);
    public Task<byte[]> GetTenderComparisonReportPdfAsync(Guid tenderId, CancellationToken ct = default) =>
        GetBytesAsync($"/api/tenders/{tenderId}/reports/comparison/pdf", ct);
    public Task<byte[]> GetTenderWinnerDecisionReportPdfAsync(Guid tenderId, CancellationToken ct = default) =>
        GetBytesAsync($"/api/tenders/{tenderId}/reports/winner-decision/pdf", ct);
    public async Task<GeneratedReportResultDto> SaveTenderSummaryReportAsync(Guid tenderId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/tenders/{tenderId}/reports/summary/save-to-file", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedReportResultDto>(cancellationToken: ct))!;
    }
    public async Task<GeneratedReportResultDto> SaveTenderComparisonReportAsync(Guid tenderId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/tenders/{tenderId}/reports/comparison/save-to-file", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedReportResultDto>(cancellationToken: ct))!;
    }
    public async Task<GeneratedReportResultDto> SaveTenderWinnerDecisionReportAsync(Guid tenderId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/tenders/{tenderId}/reports/winner-decision/save-to-file", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedReportResultDto>(cancellationToken: ct))!;
    }

    public async Task<PagedResult<TenderCommissionSessionSummaryDto>> GetCommissionSessionsAsync(CommissionSessionListRequest r, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["SearchTerm"] = r.SearchTerm,
            ["SessionNumber"] = r.SessionNumber,
            ["TenderNumber"] = r.TenderNumber,
            ["PurchaseFileNumber"] = r.PurchaseFileNumber,
            ["Status"] = r.Status?.ToString(),
            ["SessionDateFrom"] = r.SessionDateFrom?.ToString("O"),
            ["SessionDateTo"] = r.SessionDateTo?.ToString("O"),
            ["MemberUserId"] = r.MemberUserId?.ToString(),
            ["SortBy"] = r.SortBy,
            ["SortDescending"] = r.SortDescending.ToString(),
            ["PageNumber"] = r.PageNumber.ToString(),
            ["PageSize"] = r.PageSize.ToString()
        };
        var url = "/api/commission/sessions?" + string.Join("&", query.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
        return await GetJsonAsync<PagedResult<TenderCommissionSessionSummaryDto>>(url, ct) ?? new([], r.PageNumber, r.PageSize, 0);
    }

    public Task<TenderCommissionSessionDetailDto?> GetCommissionSessionAsync(Guid id, CancellationToken ct = default) =>
        GetJsonAsync<TenderCommissionSessionDetailDto>($"/api/commission/sessions/{id}", ct);

    public async Task<List<TenderCommissionSessionSummaryDto>> GetTenderCommissionSessionsAsync(Guid tenderId, CancellationToken ct = default) =>
        await GetJsonAsync<List<TenderCommissionSessionSummaryDto>>($"/api/tenders/{tenderId}/commission-sessions", ct) ?? [];

    public async Task<List<TenderCommissionSessionSummaryDto>> GetPurchaseFileCommissionSessionsAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        await GetJsonAsync<List<TenderCommissionSessionSummaryDto>>($"/api/purchase-files/{purchaseFileId}/commission-sessions", ct) ?? [];

    public async Task<TenderCommissionSessionDetailDto> CreateCommissionSessionAsync(CreateCommissionSessionRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync("/api/commission/sessions", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionSessionDetailDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionSessionDetailDto> CreateCommissionSessionFromTenderAsync(Guid tenderId, CreateCommissionSessionFromTenderRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/from-tender/{tenderId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionSessionDetailDto>(cancellationToken: ct))!;
    }

    public async Task ChangeCommissionSessionStatusAsync(Guid id, string action, object? body = null, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{id}/{action}", body ?? new { }, ct);
        await Ensure(response, ct);
    }

    public async Task<TenderCommissionMemberDto> AddCommissionMemberAsync(Guid sessionId, AddCommissionMemberRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/members", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionMemberDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionMemberDto> UpdateCommissionMemberAsync(Guid sessionId, Guid memberId, UpdateCommissionMemberRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/commission/sessions/{sessionId}/members/{memberId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionMemberDto>(cancellationToken: ct))!;
    }

    public async Task RemoveCommissionMemberAsync(Guid sessionId, Guid memberId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/commission/sessions/{sessionId}/members/{memberId}", ct); await Ensure(response, ct);
    }

    public async Task<TenderCommissionAgendaItemDto> AddCommissionAgendaItemAsync(Guid sessionId, AddAgendaItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/agenda", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionAgendaItemDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionAgendaItemDto> UpdateCommissionAgendaItemAsync(Guid sessionId, Guid agendaItemId, UpdateAgendaItemRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/commission/sessions/{sessionId}/agenda/{agendaItemId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionAgendaItemDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionMinuteDto> AddCommissionMinuteAsync(Guid sessionId, AddCommissionMinuteRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/minutes", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionMinuteDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionMinuteDto> UpdateCommissionMinuteAsync(Guid sessionId, Guid minuteId, UpdateCommissionMinuteRequest request, CancellationToken ct = default)
    {
        var response = await Client().PutAsJsonAsync($"/api/commission/sessions/{sessionId}/minutes/{minuteId}", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionMinuteDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionDecisionDto> AddCommissionDecisionAsync(Guid sessionId, AddCommissionDecisionRequest request, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/decisions", request, ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionDecisionDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionDecisionDto> ApproveCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/decisions/{decisionId}/approve", new ApproveCommissionDecisionRequest(), ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionDecisionDto>(cancellationToken: ct))!;
    }

    public async Task<TenderCommissionDecisionDto> RejectCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default)
    {
        var response = await Client().PostAsJsonAsync($"/api/commission/sessions/{sessionId}/decisions/{decisionId}/reject", new RejectCommissionDecisionRequest(), ct); await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<TenderCommissionDecisionDto>(cancellationToken: ct))!;
    }

    public async Task<List<CommissionAttachmentDto>> GetCommissionAttachmentsAsync(Guid sessionId, CancellationToken ct = default) =>
        await GetJsonAsync<List<CommissionAttachmentDto>>($"/api/commission/sessions/{sessionId}/attachments", ct) ?? [];

    public async Task<CommissionAttachmentDto> UploadCommissionAttachmentAsync(Guid sessionId, Stream stream, string fileName,
        string contentType, string documentType, string? description, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        form.Add(new StringContent(documentType), "documentType");
        if (!string.IsNullOrWhiteSpace(description)) form.Add(new StringContent(description), "description");
        var response = await Client().PostAsync($"/api/commission/sessions/{sessionId}/attachments/upload", form, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommissionAttachmentDto>(cancellationToken: ct))!;
    }

    public async Task DeleteCommissionAttachmentAsync(Guid sessionId, Guid attachmentId, CancellationToken ct = default)
    {
        var response = await Client().DeleteAsync($"/api/commission/sessions/{sessionId}/attachments/{attachmentId}", ct);
        await Ensure(response, ct);
    }

    public string GetCommissionAttachmentDownloadUrl(Guid sessionId, Guid attachmentId) =>
        $"/commission-attachment-download/{sessionId}/{attachmentId}";

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadCommissionAttachmentAsync(Guid sessionId, Guid attachmentId, CancellationToken ct = default)
    {
        using var response = await SendGetAsync($"/api/commission/sessions/{sessionId}/attachments/{attachmentId}/download", ct);
        await Ensure(response, ct);
        return await ReadFileResponseAsync(response, "commission-attachment", ct);
    }

    public Task<byte[]> GetCommissionMinutesReportPdfAsync(Guid sessionId, CancellationToken ct = default) =>
        GetBytesAsync($"/api/commission/sessions/{sessionId}/reports/minutes/pdf", ct);
    public Task<byte[]> GetCommissionDecisionReportPdfAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default) =>
        GetBytesAsync($"/api/commission/sessions/{sessionId}/reports/decision/{decisionId}/pdf", ct);
    public async Task<GeneratedReportResultDto> SaveCommissionMinutesReportAsync(Guid sessionId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/commission/sessions/{sessionId}/reports/minutes/save-to-file", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedReportResultDto>(cancellationToken: ct))!;
    }
    public async Task<GeneratedReportResultDto> SaveCommissionDecisionReportAsync(Guid sessionId, Guid decisionId, CancellationToken ct = default)
    {
        var response = await Client().PostAsync($"/api/commission/sessions/{sessionId}/reports/decision/{decisionId}/save-to-file", null, ct);
        await Ensure(response, ct);
        return (await response.Content.ReadFromJsonAsync<GeneratedReportResultDto>(cancellationToken: ct))!;
    }

    private sealed record IndentReference(Guid IndentId);

    private HttpClient Client()
    {
        http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(session?.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return http;
    }

    private async Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendGetAsync(requestUri, ct);
            await Ensure(response, ct);
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (HttpRequestException ex) when (ex is not ApiClientException)
        {
            throw new ApiClientException(new ApiClientError(
                ApiErrorType.NetworkError,
                "ارتباط با سرویس برقرار نشد. لطفاً اتصال و وضعیت API را بررسی کنید.",
                ex.Message));
        }
    }

    private async Task<byte[]> GetBytesAsync(string requestUri, CancellationToken ct = default)
    {
        using var response = await SendGetAsync(requestUri, ct);
        await Ensure(response, ct);
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    private async Task<HttpResponseMessage> SendGetAsync(string requestUri, CancellationToken ct)
    {
        var response = await Client().GetAsync(requestUri, ct);
        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || auth is null)
        {
            return response;
        }

        response.Dispose();
        if (!await auth.RefreshTokenAsync())
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri)
            };
        }

        return await Client().GetAsync(requestUri, ct);
    }

    private static async Task Ensure(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var detail = await response.Content.ReadAsStringAsync(ct);
        throw new ApiClientException(ApiClientErrors.FromStatusCode(response.StatusCode, detail));
    }

    private static async Task<(byte[] Content, string ContentType, string FileName)> ReadFileResponseAsync(
        HttpResponseMessage response, string fallbackFileName, CancellationToken ct)
    {
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? fallbackFileName;
        return (await response.Content.ReadAsByteArrayAsync(ct),
            response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream", fileName);
    }
}


