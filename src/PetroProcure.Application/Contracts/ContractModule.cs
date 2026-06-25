using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Contracts;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;

namespace PetroProcure.Application.Contracts;

public sealed record CreateContractCommand(CreateContractRequest Request);
public sealed record CreateContractFromTenderCommand(Guid TenderId, CreateContractFromTenderRequest Request);
public sealed record CreateContractFromTenderBidCommand(Guid TenderBidId, CreateContractFromTenderBidRequest Request);
public sealed record CreateContractFromPurchaseFileCommand(Guid PurchaseFileId, CreateContractFromPurchaseFileRequest Request);
public sealed record UpdateContractCommand(Guid Id, UpdateContractRequest Request);
public sealed record AddContractItemCommand(Guid ContractId, AddContractItemRequest Request);
public sealed record RemoveContractItemCommand(Guid ContractId, Guid ItemId);
public sealed record AddContractClauseCommand(Guid ContractId, AddContractClauseRequest Request);
public sealed record UpdateContractClauseCommand(Guid ContractId, Guid ClauseId, UpdateContractClauseRequest Request);
public sealed record RemoveContractClauseCommand(Guid ContractId, Guid ClauseId);
public sealed record ApplyContractTemplateCommand(Guid ContractId, Guid TemplateId);
public sealed record SubmitContractCommand(Guid Id, string? Comment);
public sealed record ApproveContractCommand(Guid Id, string? Comment);
public sealed record RejectContractCommand(Guid Id, string Comment);
public sealed record SignContractCommand(Guid Id, string? Comment);
public sealed record CancelContractCommand(Guid Id, string Reason);
public sealed record CreateContractTemplateCommand(CreateContractTemplateRequest Request);
public sealed record UpdateContractTemplateCommand(Guid Id, UpdateContractTemplateRequest Request);
public sealed record AddContractTemplateClauseCommand(Guid TemplateId, AddContractTemplateClauseRequest Request);
public sealed record UpdateContractTemplateClauseCommand(Guid TemplateId, Guid ClauseId, UpdateContractTemplateClauseRequest Request);

public sealed record GetContractsQuery(ContractListRequest Request);
public sealed record GetContractByIdQuery(Guid Id);
public sealed record GetContractByNumberQuery(string ContractNumber);
public sealed record GetContractsByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetContractsBySupplierQuery(Guid SupplierId);
public sealed record GetContractTemplatesQuery(bool IncludeInactive = false);
public sealed record GetContractTemplateByIdQuery(Guid Id);
public sealed record GetContractDocumentsQuery(Guid ContractId);

public sealed record ContractItemSnapshot(Guid? PurchaseFileItemId, Guid? TenderBidItemId, Guid MescItemId,
    string MescCode, string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription,
    Guid UnitOfMeasureId, decimal Quantity, decimal? UnitPrice, DateTime? DeliveryDate, string? TechnicalDescription);

public sealed record TenderContractSnapshot(Guid TenderId, Guid PurchaseFileId, string TenderNumber,
    Guid? SelectedTenderBidId, Guid? SelectedSupplierId, Guid? CommissionDecisionId, IReadOnlyList<ContractItemSnapshot> Items);

public interface IContractNumberService
{
    Task<string> GenerateNextContractNumber(int year, CancellationToken ct = default);
}

public interface IContractEligibilityService
{
    Task EnsureCanCreateForPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task EnsureSupplierExistsAsync(Guid supplierId, CancellationToken ct = default);
}

public interface IContractTemplateService
{
    Task ApplyTemplateAsync(PurchaseContract contract, Guid templateId, Guid userId, CancellationToken ct = default);
}

public interface IContractReportDataSourceBuilder
{
    Task<PurchaseContractDetailDto?> BuildAsync(Guid contractId, CancellationToken ct = default);
}

public interface IContractRepository
{
    Task<string> GenerateNextContractNumberAsync(int year, CancellationToken ct);
    Task<bool> ContractNumberExistsAsync(string contractNumber, CancellationToken ct);
    Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct);
    Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct);
    Task<PurchaseContract?> FindContractAsync(Guid id, bool includeDetails, CancellationToken ct);
    Task<ContractClause?> FindClauseAsync(Guid contractId, Guid clauseId, CancellationToken ct);
    Task<ContractTemplate?> FindTemplateAsync(Guid id, bool includeClauses, CancellationToken ct);
    Task<ContractTemplateClause?> FindTemplateClauseAsync(Guid templateId, Guid clauseId, CancellationToken ct);
    Task<bool> TemplateCodeExistsAsync(string templateCode, Guid? excludingId, CancellationToken ct);
    Task<IReadOnlyList<ContractItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct);
    Task<ContractItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct);
    Task<TenderContractSnapshot?> GetTenderSnapshotAsync(Guid tenderId, Guid? supplierId, Guid? tenderBidId, CancellationToken ct);
    Task<TenderContractSnapshot?> GetTenderBidSnapshotAsync(Guid tenderBidId, CancellationToken ct);
    Task AddContractAsync(PurchaseContract contract, CancellationToken ct);
    Task AddTemplateAsync(ContractTemplate template, CancellationToken ct);
    Task AddContractDocumentAsync(ContractDocument document, CancellationToken ct);
    Task<PagedResult<PurchaseContractSummaryDto>> GetContractsAsync(ContractListRequest request, CancellationToken ct);
    Task<PurchaseContractDetailDto?> GetContractDetailAsync(Guid id, CancellationToken ct);
    Task<PurchaseContractDetailDto?> GetContractDetailByNumberAsync(string contractNumber, CancellationToken ct);
    Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct);
    Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsBySupplierAsync(Guid supplierId, CancellationToken ct);
    Task<IReadOnlyList<ContractTemplateDto>> GetTemplatesAsync(bool includeInactive, CancellationToken ct);
    Task<ContractTemplateDto?> GetTemplateAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ContractDocumentDto>> GetDocumentsAsync(Guid contractId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class ContractNumberService(IContractRepository repository) : IContractNumberService
{
    public async Task<string> GenerateNextContractNumber(int year, CancellationToken ct = default)
    {
        if (year < 2000 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        var number = await repository.GenerateNextContractNumberAsync(year, ct);
        if (!number.StartsWith($"CNT-{year:0000}-", StringComparison.Ordinal) || number.Length != 15)
            throw new ContractValidationException("Generated contract number has invalid format.");
        return number;
    }
}

public sealed class ContractEligibilityService(IContractRepository repository) : IContractEligibilityService
{
    public async Task EnsureCanCreateForPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct = default)
    {
        if (!await repository.PurchaseFileExistsAsync(purchaseFileId, ct))
            throw new ContractValidationException("Purchase file was not found.");
    }

    public async Task EnsureSupplierExistsAsync(Guid supplierId, CancellationToken ct = default)
    {
        if (supplierId == Guid.Empty || !await repository.SupplierExistsAsync(supplierId, ct))
            throw new ContractValidationException("Supplier was not found.");
    }
}

public sealed class ContractTemplateService(IContractRepository repository) : IContractTemplateService
{
    public async Task ApplyTemplateAsync(PurchaseContract contract, Guid templateId, Guid userId, CancellationToken ct = default)
    {
        var template = await repository.FindTemplateAsync(templateId, true, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
        if (!template.IsActive) throw new ContractValidationException("Inactive contract templates cannot be applied.");
        contract.ApplyTemplate(template.Id);
        foreach (var clause in template.Clauses.OrderBy(x => x.OrderNo))
        {
            contract.AddClause(new ContractClause(Guid.NewGuid(), contract.Id, clause.OrderNo, clause.Title,
                clause.Body, clause.ClauseType, clause.IsRequired, clause.IsEditable, userId));
        }
    }
}

public sealed class ContractReportDataSourceBuilder(IContractRepository repository) : IContractReportDataSourceBuilder
{
    public Task<PurchaseContractDetailDto?> BuildAsync(Guid contractId, CancellationToken ct = default) =>
        repository.GetContractDetailAsync(contractId, ct);
}

public sealed class ContractCommandHandler(
    IContractRepository repository,
    IContractNumberService numberService,
    IContractEligibilityService eligibility,
    IContractTemplateService templateService,
    ICurrentUserService currentUser)
{
    public async Task<PurchaseContractDetailDto> Handle(CreateContractCommand command, CancellationToken ct = default)
    {
        var request = command.Request;
        await eligibility.EnsureCanCreateForPurchaseFileAsync(request.PurchaseFileId, ct);
        await eligibility.EnsureSupplierExistsAsync(request.SupplierId, ct);
        var contract = await NewContract(
            request.PurchaseFileId, request.SupplierId, request.TenderId, request.TenderBidId,
            request.CommissionDecisionId, request.ContractTemplateId, request.Title, request.Subject,
            request.ContractType, request.Currency, request.TotalAmount, request.TaxAmount, request.FinalAmount,
            request.StartDate, request.EndDate, request.DeliveryDeadline, request.PaymentTerms, request.DeliveryTerms,
            request.WarrantyTerms, request.PenaltyTerms, request.Description, ct);
        await repository.AddContractAsync(contract, ct);
        if (request.ContractTemplateId.HasValue)
            await templateService.ApplyTemplateAsync(contract, request.ContractTemplateId.Value, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public async Task<PurchaseContractDetailDto> Handle(CreateContractFromPurchaseFileCommand command, CancellationToken ct = default)
    {
        await eligibility.EnsureCanCreateForPurchaseFileAsync(command.PurchaseFileId, ct);
        await eligibility.EnsureSupplierExistsAsync(command.Request.SupplierId, ct);
        var contract = await NewContract(command.PurchaseFileId, command.Request.SupplierId, null, null, null,
            command.Request.ContractTemplateId, command.Request.Title, command.Request.Subject,
            command.Request.ContractType, "IRR", null, null, null, null, null, null, null, null, null, null,
            "قرارداد ایجادشده از پرونده خرید", ct);
        foreach (var item in await repository.GetPurchaseFileItemSnapshotsAsync(command.PurchaseFileId, ct))
            contract.AddItem(ToContractItem(contract.Id, item));
        await repository.AddContractAsync(contract, ct);
        if (command.Request.ContractTemplateId.HasValue)
            await templateService.ApplyTemplateAsync(contract, command.Request.ContractTemplateId.Value, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public async Task<PurchaseContractDetailDto> Handle(CreateContractFromTenderCommand command, CancellationToken ct = default)
    {
        var snapshot = await repository.GetTenderSnapshotAsync(command.TenderId, command.Request.SupplierId, command.Request.TenderBidId, ct)
            ?? throw new ContractValidationException("Tender was not found or has no selectable supplier.");
        var supplierId = command.Request.SupplierId ?? snapshot.SelectedSupplierId
            ?? throw new ContractValidationException("Tender contract requires selected supplier.");
        await eligibility.EnsureSupplierExistsAsync(supplierId, ct);
        var contract = await NewContract(snapshot.PurchaseFileId, supplierId, snapshot.TenderId,
            command.Request.TenderBidId ?? snapshot.SelectedTenderBidId, snapshot.CommissionDecisionId,
            command.Request.ContractTemplateId, command.Request.Title ?? $"قرارداد مناقصه {snapshot.TenderNumber}",
            command.Request.Subject ?? $"قرارداد ناشی از مناقصه {snapshot.TenderNumber}", ContractType.TenderBased,
            "IRR", null, null, null, null, null, null, null, null, null, null, null, ct);
        foreach (var item in snapshot.Items)
            contract.AddItem(ToContractItem(contract.Id, item));
        await repository.AddContractAsync(contract, ct);
        if (command.Request.ContractTemplateId.HasValue)
            await templateService.ApplyTemplateAsync(contract, command.Request.ContractTemplateId.Value, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public async Task<PurchaseContractDetailDto> Handle(CreateContractFromTenderBidCommand command, CancellationToken ct = default)
    {
        var snapshot = await repository.GetTenderBidSnapshotAsync(command.TenderBidId, ct)
            ?? throw new ContractValidationException("Tender bid was not found.");
        var supplierId = snapshot.SelectedSupplierId
            ?? throw new ContractValidationException("Tender bid has no supplier.");
        var contract = await NewContract(snapshot.PurchaseFileId, supplierId, snapshot.TenderId,
            command.TenderBidId, snapshot.CommissionDecisionId, command.Request.ContractTemplateId,
            command.Request.Title ?? $"قرارداد پیشنهاد {snapshot.TenderNumber}",
            command.Request.Subject ?? $"قرارداد براساس پیشنهاد منتخب مناقصه {snapshot.TenderNumber}",
            ContractType.TenderBased, "IRR", null, null, null, null, null, null, null, null, null, null, null, ct);
        foreach (var item in snapshot.Items)
            contract.AddItem(ToContractItem(contract.Id, item));
        await repository.AddContractAsync(contract, ct);
        if (command.Request.ContractTemplateId.HasValue)
            await templateService.ApplyTemplateAsync(contract, command.Request.ContractTemplateId.Value, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public async Task<PurchaseContractDetailDto> Handle(UpdateContractCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.Id, true, ct);
        var request = command.Request;
        contract.Update(request.Title, request.Subject, request.ContractType, request.Currency, request.TotalAmount,
            request.TaxAmount, request.FinalAmount, request.StartDate, request.EndDate, request.DeliveryDeadline,
            request.PaymentTerms, request.DeliveryTerms, request.WarrantyTerms, request.PenaltyTerms, request.Description);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public async Task<PurchaseContractItemDto> Handle(AddContractItemCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        ContractItemSnapshot snapshot = command.Request.PurchaseFileItemId.HasValue
            ? await repository.GetPurchaseFileItemSnapshotAsync(command.Request.PurchaseFileItemId.Value, ct)
                ?? FromRequest(command.Request)
            : FromRequest(command.Request);
        contract.AddItem(ToContractItem(contract.Id, snapshot));
        await repository.SaveChangesAsync(ct);
        return (await RequiredDetail(contract.Id, ct)).Items.OrderByDescending(x => x.Id).First();
    }

    public async Task Handle(RemoveContractItemCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        contract.RemoveItem(command.ItemId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<ContractClauseDto> Handle(AddContractClauseCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        var request = command.Request;
        var clause = new ContractClause(Guid.NewGuid(), contract.Id, request.OrderNo, request.Title,
            request.Body, request.ClauseType, request.IsRequired, request.IsEditable, currentUser.UserId);
        contract.AddClause(clause);
        await repository.SaveChangesAsync(ct);
        return (await RequiredDetail(contract.Id, ct)).Clauses.Single(x => x.Id == clause.Id);
    }

    public async Task<ContractClauseDto> Handle(UpdateContractClauseCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        contract.EnsureEditable();
        var clause = await repository.FindClauseAsync(command.ContractId, command.ClauseId, ct)
            ?? throw new ContractNotFoundException("Contract clause was not found.");
        var request = command.Request;
        clause.Update(request.OrderNo, request.Title, request.Body, request.ClauseType, request.IsRequired,
            request.IsEditable, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
        return (await RequiredDetail(contract.Id, ct)).Clauses.Single(x => x.Id == clause.Id);
    }

    public async Task Handle(RemoveContractClauseCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        contract.RemoveClause(command.ClauseId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<PurchaseContractDetailDto> Handle(ApplyContractTemplateCommand command, CancellationToken ct = default)
    {
        var contract = await RequiredContract(command.ContractId, true, ct);
        await templateService.ApplyTemplateAsync(contract, command.TemplateId, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(contract.Id, ct);
    }

    public Task Handle(SubmitContractCommand command, CancellationToken ct = default) =>
        Change(command.Id, contract => contract.Submit(currentUser.UserId), ct);
    public Task Handle(ApproveContractCommand command, CancellationToken ct = default) =>
        Change(command.Id, contract =>
        {
            contract.Approve(currentUser.UserId);
            contract.AddApproval(new ContractApproval(Guid.NewGuid(), contract.Id, "ContractApproval", null, currentUser.UserId, command.Comment));
            contract.Approvals.Last().Approve(command.Comment);
        }, ct);
    public Task Handle(RejectContractCommand command, CancellationToken ct = default) =>
        Change(command.Id, contract => contract.Reject(currentUser.UserId, command.Comment), ct);
    public Task Handle(SignContractCommand command, CancellationToken ct = default) =>
        Change(command.Id, contract => contract.Sign(currentUser.UserId), ct);
    public Task Handle(CancelContractCommand command, CancellationToken ct = default) =>
        Change(command.Id, contract => contract.Cancel(command.Reason, currentUser.UserId), ct);

    public async Task<ContractTemplateDto> Handle(CreateContractTemplateCommand command, CancellationToken ct = default)
    {
        var request = command.Request;
        if (await repository.TemplateCodeExistsAsync(request.TemplateCode.Trim(), null, ct))
            throw new ContractConflictException("Contract template code already exists.");
        var template = new ContractTemplate(Guid.NewGuid(), request.TemplateCode, request.Title,
            request.Description, request.ContractType, currentUser.UserId);
        await repository.AddTemplateAsync(template, ct);
        foreach (var clause in request.Clauses.OrderBy(x => x.OrderNo))
            template.AddClause(new ContractTemplateClause(Guid.NewGuid(), template.Id, clause.OrderNo,
                clause.Title, clause.Body, clause.ClauseType, clause.IsRequired, clause.IsEditable));
        await repository.SaveChangesAsync(ct);
        return await repository.GetTemplateAsync(template.Id, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
    }

    public async Task<ContractTemplateDto> Handle(UpdateContractTemplateCommand command, CancellationToken ct = default)
    {
        var template = await repository.FindTemplateAsync(command.Id, true, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
        template.Update(command.Request.Title, command.Request.Description, command.Request.ContractType, command.Request.IsActive);
        await repository.SaveChangesAsync(ct);
        return await repository.GetTemplateAsync(template.Id, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
    }

    public async Task<ContractTemplateClauseDto> Handle(AddContractTemplateClauseCommand command, CancellationToken ct = default)
    {
        var template = await repository.FindTemplateAsync(command.TemplateId, true, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
        var request = command.Request;
        var clause = new ContractTemplateClause(Guid.NewGuid(), template.Id, request.OrderNo,
            request.Title, request.Body, request.ClauseType, request.IsRequired, request.IsEditable);
        template.AddClause(clause);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTemplateAsync(template.Id, ct))!.Clauses.Single(x => x.Id == clause.Id);
    }

    public async Task<ContractTemplateClauseDto> Handle(UpdateContractTemplateClauseCommand command, CancellationToken ct = default)
    {
        _ = await repository.FindTemplateAsync(command.TemplateId, true, ct)
            ?? throw new ContractNotFoundException("Contract template was not found.");
        var clause = await repository.FindTemplateClauseAsync(command.TemplateId, command.ClauseId, ct)
            ?? throw new ContractNotFoundException("Contract template clause was not found.");
        var request = command.Request;
        clause.Update(request.OrderNo, request.Title, request.Body, request.ClauseType, request.IsRequired, request.IsEditable);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTemplateAsync(command.TemplateId, ct))!.Clauses.Single(x => x.Id == clause.Id);
    }

    private async Task<PurchaseContract> NewContract(Guid purchaseFileId, Guid supplierId, Guid? tenderId,
        Guid? tenderBidId, Guid? commissionDecisionId, Guid? templateId, string title, string subject,
        ContractType type, string currency, decimal? total, decimal? tax, decimal? final, DateTime? start,
        DateTime? end, DateTime? deadline, string? payment, string? delivery, string? warranty,
        string? penalty, string? description, CancellationToken ct)
    {
        EnsureAuthenticated();
        var number = await numberService.GenerateNextContractNumber(DateTime.UtcNow.Year, ct);
        if (await repository.ContractNumberExistsAsync(number, ct))
            throw new ContractConflictException("Contract number already exists.");
        return new PurchaseContract(Guid.NewGuid(), number, purchaseFileId, supplierId, tenderId, tenderBidId,
            commissionDecisionId, templateId, title, subject, type, currency, currentUser.UserId,
            total, tax, final, start, end, deadline, payment, delivery, warranty, penalty, description);
    }

    private async Task Change(Guid id, Action<PurchaseContract> action, CancellationToken ct)
    {
        var contract = await RequiredContract(id, true, ct);
        try { action(contract); }
        catch (InvalidOperationException ex) { throw new ContractValidationException(ex.Message); }
        await repository.SaveChangesAsync(ct);
    }

    private async Task<PurchaseContract> RequiredContract(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindContractAsync(id, includeDetails, ct)
        ?? throw new ContractNotFoundException("Contract was not found.");

    private async Task<PurchaseContractDetailDto> RequiredDetail(Guid id, CancellationToken ct) =>
        await repository.GetContractDetailAsync(id, ct)
        ?? throw new ContractNotFoundException("Contract was not found.");

    private void EnsureAuthenticated()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
            throw new CurrentUserNotAuthenticatedException();
    }

    private static PurchaseContractItem ToContractItem(Guid contractId, ContractItemSnapshot item) =>
        new(Guid.NewGuid(), contractId, item.PurchaseFileItemId, item.TenderBidItemId, item.MescItemId,
            item.MescCode, item.MescGeneralGroupCode, item.GeneralDescription, item.SpecificDescription,
            item.UnitOfMeasureId, item.Quantity, item.UnitPrice, item.DeliveryDate, item.TechnicalDescription);

    private static ContractItemSnapshot FromRequest(AddContractItemRequest request) =>
        new(request.PurchaseFileItemId, request.TenderBidItemId, request.MescItemId, request.MescCode,
            request.MescGeneralGroupCode, request.GeneralDescription, request.SpecificDescription,
            request.UnitOfMeasureId, request.Quantity, request.UnitPrice, request.DeliveryDate, request.TechnicalDescription);
}

public sealed class ContractQueryHandler(IContractRepository repository)
{
    public Task<PagedResult<PurchaseContractSummaryDto>> Handle(GetContractsQuery query, CancellationToken ct = default) =>
        repository.GetContractsAsync(query.Request, ct);
    public Task<PurchaseContractDetailDto?> Handle(GetContractByIdQuery query, CancellationToken ct = default) =>
        repository.GetContractDetailAsync(query.Id, ct);
    public Task<PurchaseContractDetailDto?> Handle(GetContractByNumberQuery query, CancellationToken ct = default) =>
        repository.GetContractDetailByNumberAsync(query.ContractNumber, ct);
    public Task<IReadOnlyList<PurchaseContractSummaryDto>> Handle(GetContractsByPurchaseFileQuery query, CancellationToken ct = default) =>
        repository.GetContractsByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<IReadOnlyList<PurchaseContractSummaryDto>> Handle(GetContractsBySupplierQuery query, CancellationToken ct = default) =>
        repository.GetContractsBySupplierAsync(query.SupplierId, ct);
    public Task<IReadOnlyList<ContractTemplateDto>> Handle(GetContractTemplatesQuery query, CancellationToken ct = default) =>
        repository.GetTemplatesAsync(query.IncludeInactive, ct);
    public Task<ContractTemplateDto?> Handle(GetContractTemplateByIdQuery query, CancellationToken ct = default) =>
        repository.GetTemplateAsync(query.Id, ct);
    public Task<IReadOnlyList<ContractDocumentDto>> Handle(GetContractDocumentsQuery query, CancellationToken ct = default) =>
        repository.GetDocumentsAsync(query.ContractId, ct);
}

public sealed class ContractValidationException(string message) : Exception(message);
public sealed class ContractNotFoundException(string message) : Exception(message);
public sealed class ContractConflictException(string message) : Exception(message);
