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
        Client().GetByteArrayAsync($"/api/reports/indent/{id}/pdf", ct);

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
        Client().GetByteArrayAsync($"/api/reports/purchase-file-summary/{id}/pdf", ct);

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
        var response = await Client().GetAsync($"/api/documents/{id}/download", ct); await Ensure(response, ct);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "document";
        return (await response.Content.ReadAsByteArrayAsync(ct),
            response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream", fileName);
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
}


