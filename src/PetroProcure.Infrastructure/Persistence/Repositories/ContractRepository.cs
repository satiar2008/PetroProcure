using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Contracts;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Contracts;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class ContractRepository(PetroProcureDbContext db) : IContractRepository
{
    public async Task<string> GenerateNextContractNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.ContractSequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null)
            {
                next = 1;
                db.ContractSequences.Add(new ContractSequence(Guid.NewGuid(), year, next));
            }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return $"CNT-{year:0000}-{next:000000}";
        });
    }

    public Task<bool> ContractNumberExistsAsync(string contractNumber, CancellationToken ct) =>
        db.PurchaseContracts.AnyAsync(x => x.ContractNumber == contractNumber, ct);

    public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) =>
        db.PurchaseFiles.AnyAsync(x => x.Id == purchaseFileId, ct);

    public Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct) =>
        db.Suppliers.AnyAsync(x => x.Id == supplierId && x.IsActive && !x.IsBlacklisted, ct);

    public Task<PurchaseContract?> FindContractAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<PurchaseContract> query = db.PurchaseContracts;
        if (includeDetails)
        {
            query = query.Include(x => x.Items).Include(x => x.Clauses)
                .Include(x => x.Approvals).Include(x => x.Documents);
        }
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<ContractClause?> FindClauseAsync(Guid contractId, Guid clauseId, CancellationToken ct) =>
        db.ContractClauses.SingleOrDefaultAsync(x => x.ContractId == contractId && x.Id == clauseId, ct);

    public Task<ContractTemplate?> FindTemplateAsync(Guid id, bool includeClauses, CancellationToken ct)
    {
        IQueryable<ContractTemplate> query = db.ContractTemplates;
        if (includeClauses) query = query.Include(x => x.Clauses);
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<ContractTemplateClause?> FindTemplateClauseAsync(Guid templateId, Guid clauseId, CancellationToken ct) =>
        db.ContractTemplateClauses.SingleOrDefaultAsync(x => x.TemplateId == templateId && x.Id == clauseId, ct);

    public Task<bool> TemplateCodeExistsAsync(string templateCode, Guid? excludingId, CancellationToken ct) =>
        db.ContractTemplates.AnyAsync(x => x.TemplateCode == templateCode && (!excludingId.HasValue || x.Id != excludingId), ct);

    public async Task<IReadOnlyList<ContractItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct) =>
        await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderBy(x => x.MescCode)
            .Select(x => new ContractItemSnapshot(x.Id, null, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, null, null, x.TechnicalDescription))
            .ToListAsync(ct);

    public Task<ContractItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct) =>
        db.PurchaseFileItems.AsNoTracking().Where(x => x.Id == purchaseFileItemId)
            .Select(x => new ContractItemSnapshot(x.Id, null, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, null, null, x.TechnicalDescription))
            .SingleOrDefaultAsync(ct);

    public async Task<TenderContractSnapshot?> GetTenderSnapshotAsync(Guid tenderId, Guid? supplierId, Guid? tenderBidId, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderId, ct);
        if (tender is null) return null;
        var selectedBid = tenderBidId.HasValue
            ? await db.TenderBids.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderBidId && x.TenderId == tenderId, ct)
            : await db.TenderBids.AsNoTracking()
                .Where(x => x.TenderId == tenderId && (x.Status == TenderBidStatus.Selected || x.SupplierId == supplierId))
                .OrderByDescending(x => x.Status == TenderBidStatus.Selected)
                .FirstOrDefaultAsync(ct);
        var decision = await db.TenderCommissionDecisions.AsNoTracking()
            .Where(x => x.TenderId == tenderId && x.Status == TenderCommissionDecisionStatus.Approved)
            .OrderByDescending(x => x.ApprovedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        var selectedSupplier = supplierId ?? selectedBid?.SupplierId ?? decision?.SelectedSupplierId;
        var items = selectedBid is not null
            ? await BidItems(selectedBid.Id, ct)
            : await TenderItems(tenderId, ct);
        return new TenderContractSnapshot(tender.Id, tender.PurchaseFileId, tender.TenderNumber,
            selectedBid?.Id ?? decision?.SelectedTenderBidId, selectedSupplier, decision?.Id, items);
    }

    public async Task<TenderContractSnapshot?> GetTenderBidSnapshotAsync(Guid tenderBidId, CancellationToken ct)
    {
        var bid = await db.TenderBids.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderBidId, ct);
        if (bid is null) return null;
        var tender = await db.Tenders.AsNoTracking().SingleAsync(x => x.Id == bid.TenderId, ct);
        var decision = await db.TenderCommissionDecisions.AsNoTracking()
            .Where(x => x.SelectedTenderBidId == tenderBidId && x.Status == TenderCommissionDecisionStatus.Approved)
            .OrderByDescending(x => x.ApprovedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return new TenderContractSnapshot(tender.Id, tender.PurchaseFileId, tender.TenderNumber,
            bid.Id, bid.SupplierId, decision?.Id, await BidItems(bid.Id, ct));
    }

    public async Task AddContractAsync(PurchaseContract contract, CancellationToken ct) =>
        await db.PurchaseContracts.AddAsync(contract, ct);

    public async Task AddTemplateAsync(ContractTemplate template, CancellationToken ct) =>
        await db.ContractTemplates.AddAsync(template, ct);

    public async Task AddContractDocumentAsync(ContractDocument document, CancellationToken ct) =>
        await db.ContractDocuments.AddAsync(document, ct);

    public async Task<PagedResult<PurchaseContractSummaryDto>> GetContractsAsync(ContractListRequest request, CancellationToken ct)
    {
        var query = ApplyFilters(ContractSummaryProjectionQuery(), request);
        var total = await query.LongCountAsync(ct);
        var items = await ApplySort(query, request.SortBy, request.SortDescending)
            .Skip((Math.Max(1, request.PageNumber) - 1) * Math.Max(1, request.PageSize))
            .Take(Math.Clamp(request.PageSize, 1, 200))
            .Select(x => new PurchaseContractSummaryDto(x.Id, x.ContractNumber, x.PurchaseFileId,
                x.PurchaseFileNumber, x.SupplierId, x.SupplierName, x.Title, x.ContractType,
                x.Status, x.FinalAmount, x.Currency, x.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<PurchaseContractSummaryDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PurchaseContractDetailDto?> GetContractDetailAsync(Guid id, CancellationToken ct)
    {
        var contract = await ContractDtoQuery(id: id).SingleOrDefaultAsync(ct);
        return contract is null ? null : await Detail(contract, ct);
    }

    public async Task<PurchaseContractDetailDto?> GetContractDetailByNumberAsync(string contractNumber, CancellationToken ct)
    {
        var contract = await ContractDtoQuery(contractNumber: contractNumber).SingleOrDefaultAsync(ct);
        return contract is null ? null : await Detail(contract, ct);
    }

    public async Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        await ContractSummaryProjectionQuery().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PurchaseContractSummaryDto(x.Id, x.ContractNumber, x.PurchaseFileId,
                x.PurchaseFileNumber, x.SupplierId, x.SupplierName, x.Title, x.ContractType,
                x.Status, x.FinalAmount, x.Currency, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsBySupplierAsync(Guid supplierId, CancellationToken ct) =>
        await ContractSummaryProjectionQuery().Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PurchaseContractSummaryDto(x.Id, x.ContractNumber, x.PurchaseFileId,
                x.PurchaseFileNumber, x.SupplierId, x.SupplierName, x.Title, x.ContractType,
                x.Status, x.FinalAmount, x.Currency, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ContractTemplateDto>> GetTemplatesAsync(bool includeInactive, CancellationToken ct)
    {
        var templates = await db.ContractTemplates.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.TemplateCode)
            .ToListAsync(ct);
        var clauses = await db.ContractTemplateClauses.AsNoTracking()
            .Where(x => templates.Select(t => t.Id).Contains(x.TemplateId))
            .OrderBy(x => x.OrderNo)
            .Select(x => new ContractTemplateClauseDto(x.Id, x.TemplateId, x.OrderNo, x.Title,
                x.Body, x.ClauseType, x.IsRequired, x.IsEditable))
            .ToListAsync(ct);
        return templates.Select(x => new ContractTemplateDto(x.Id, x.TemplateCode, x.Title, x.Description,
            x.ContractType, x.IsActive, x.CreatedAt, x.CreatedByUserId,
            clauses.Where(c => c.TemplateId == x.Id).ToArray())).ToArray();
    }

    public async Task<ContractTemplateDto?> GetTemplateAsync(Guid id, CancellationToken ct) =>
        (await GetTemplatesAsync(true, ct)).SingleOrDefault(x => x.Id == id);

    public async Task<IReadOnlyList<ContractDocumentDto>> GetDocumentsAsync(Guid contractId, CancellationToken ct) =>
        await db.ContractDocuments.AsNoTracking().Where(x => x.ContractId == contractId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new ContractDocumentDto(x.Id, x.ContractId, x.FileDocumentId, x.DocumentType,
                x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private async Task<PurchaseContractDetailDto> Detail(PurchaseContractDto contract, CancellationToken ct)
    {
        var items = await db.PurchaseContractItems.AsNoTracking().Where(x => x.ContractId == contract.Id)
            .OrderBy(x => x.MescGeneralGroupCode).ThenBy(x => x.MescCode)
            .Select(x => new PurchaseContractItemDto(x.Id, x.ContractId, x.PurchaseFileItemId, x.TenderBidItemId,
                x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
                x.UnitOfMeasureId, x.Quantity, x.UnitPrice, x.TotalPrice, x.DeliveryDate, x.TechnicalDescription))
            .ToListAsync(ct);
        var clauses = await db.ContractClauses.AsNoTracking().Where(x => x.ContractId == contract.Id)
            .OrderBy(x => x.OrderNo)
            .Select(x => new ContractClauseDto(x.Id, x.ContractId, x.OrderNo, x.Title, x.Body, x.ClauseType,
                x.IsRequired, x.IsEditable, x.CreatedAt, x.CreatedByUserId, x.UpdatedAt, x.UpdatedByUserId))
            .ToListAsync(ct);
        var approvals = await db.ContractApprovals.AsNoTracking().Where(x => x.ContractId == contract.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ContractApprovalDto(x.Id, x.ContractId, x.ApprovalStep, x.DepartmentId,
                x.ApproverUserId, x.Status, x.Comment, x.CreatedAt, x.ApprovedAt, x.RejectedAt))
            .ToListAsync(ct);
        var documents = await GetDocumentsAsync(contract.Id, ct);
        return new PurchaseContractDetailDto(contract, items, clauses, approvals, documents);
    }

    private IQueryable<ContractSummaryProjection> ContractSummaryProjectionQuery() =>
        from contract in db.PurchaseContracts.AsNoTracking()
        join file in db.PurchaseFiles.AsNoTracking() on contract.PurchaseFileId equals file.Id
        join supplier in db.Suppliers.AsNoTracking() on contract.SupplierId equals supplier.Id
        join tender in db.Tenders.AsNoTracking() on contract.TenderId equals tender.Id into tenders
        from tender in tenders.DefaultIfEmpty()
        select new ContractSummaryProjection
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            PurchaseFileId = file.Id,
            PurchaseFileNumber = file.FileNumber,
            TenderNumber = tender == null ? null : tender.TenderNumber,
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            Title = contract.Title,
            ContractType = contract.ContractType,
            Status = contract.Status,
            FinalAmount = contract.FinalAmount,
            Currency = contract.Currency,
            CreatedAt = contract.CreatedAt
        };

    private IQueryable<PurchaseContractDto> ContractDtoQuery(Guid? id = null, string? contractNumber = null)
    {
        var contracts = db.PurchaseContracts.AsNoTracking();
        if (id.HasValue) contracts = contracts.Where(x => x.Id == id.Value);
        if (!string.IsNullOrWhiteSpace(contractNumber)) contracts = contracts.Where(x => x.ContractNumber == contractNumber);

        return from contract in contracts
        join file in db.PurchaseFiles.AsNoTracking() on contract.PurchaseFileId equals file.Id
        join supplier in db.Suppliers.AsNoTracking() on contract.SupplierId equals supplier.Id
        join tender in db.Tenders.AsNoTracking() on contract.TenderId equals tender.Id into tenders
        from tender in tenders.DefaultIfEmpty()
        select new PurchaseContractDto(contract.Id, contract.ContractNumber, contract.PurchaseFileId, file.FileNumber,
            contract.SupplierId, supplier.Name, contract.TenderId, tender == null ? null : tender.TenderNumber,
            contract.TenderBidId, contract.CommissionDecisionId, contract.ContractTemplateId,
            contract.Title, contract.Subject, contract.Status, contract.ContractType, contract.Currency,
            contract.TotalAmount, contract.TaxAmount, contract.FinalAmount, contract.StartDate, contract.EndDate,
            contract.DeliveryDeadline, contract.PaymentTerms, contract.DeliveryTerms, contract.WarrantyTerms,
            contract.PenaltyTerms, contract.Description, contract.CreatedAt, contract.CreatedByUserId,
            contract.SubmittedAt, contract.SubmittedByUserId, contract.ApprovedAt, contract.ApprovedByUserId,
            contract.SignedAt, contract.SignedByUserId, contract.CancelledAt, contract.CancelledByUserId,
            contract.CancellationReason);
    }

    private IQueryable<ContractSummaryProjection> ApplyFilters(IQueryable<ContractSummaryProjection> query, ContractListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.ContractNumber.Contains(term) || x.Title.Contains(term)
                || x.SupplierName.Contains(term) || x.PurchaseFileNumber.Contains(term)
                || (x.TenderNumber != null && x.TenderNumber.Contains(term)));
        }
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.ContractType.HasValue) query = query.Where(x => x.ContractType == request.ContractType);
        if (request.SupplierId.HasValue) query = query.Where(x => x.SupplierId == request.SupplierId);
        if (!string.IsNullOrWhiteSpace(request.PurchaseFileNumber))
            query = query.Where(x => x.PurchaseFileNumber.Contains(request.PurchaseFileNumber));
        if (!string.IsNullOrWhiteSpace(request.TenderNumber))
            query = query.Where(x => x.TenderNumber != null && x.TenderNumber.Contains(request.TenderNumber));
        if (request.CreatedDateFrom.HasValue) query = query.Where(x => x.CreatedAt >= request.CreatedDateFrom);
        if (request.CreatedDateTo.HasValue) query = query.Where(x => x.CreatedAt < request.CreatedDateTo.Value.Date.AddDays(1));
        return query;
    }

    private static IQueryable<ContractSummaryProjection> ApplySort(IQueryable<ContractSummaryProjection> query, string? sortBy, bool desc) =>
        (sortBy ?? "CreatedAt").ToLowerInvariant() switch
        {
            "contractnumber" => desc ? query.OrderByDescending(x => x.ContractNumber) : query.OrderBy(x => x.ContractNumber),
            "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "supplier" => desc ? query.OrderByDescending(x => x.SupplierName) : query.OrderBy(x => x.SupplierName),
            "finalamount" => desc ? query.OrderByDescending(x => x.FinalAmount) : query.OrderBy(x => x.FinalAmount),
            _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

    private sealed class ContractSummaryProjection
    {
        public Guid Id { get; init; }
        public string ContractNumber { get; init; } = "";
        public Guid PurchaseFileId { get; init; }
        public string PurchaseFileNumber { get; init; } = "";
        public string? TenderNumber { get; init; }
        public Guid SupplierId { get; init; }
        public string SupplierName { get; init; } = "";
        public string Title { get; init; } = "";
        public ContractType ContractType { get; init; }
        public ContractStatus Status { get; init; }
        public decimal? FinalAmount { get; init; }
        public string Currency { get; init; } = "";
        public DateTime CreatedAt { get; init; }
    }

    private async Task<IReadOnlyList<ContractItemSnapshot>> TenderItems(Guid tenderId, CancellationToken ct) =>
        await db.TenderItems.AsNoTracking().Where(x => x.TenderId == tenderId)
            .OrderBy(x => x.MescCode)
            .Select(x => new ContractItemSnapshot(x.PurchaseFileItemId, null, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.Quantity, null, null, x.TechnicalDescription))
            .ToListAsync(ct);

    private async Task<IReadOnlyList<ContractItemSnapshot>> BidItems(Guid tenderBidId, CancellationToken ct) =>
        await (from bidItem in db.TenderBidItems.AsNoTracking()
               join tenderItem in db.TenderItems.AsNoTracking() on bidItem.TenderItemId equals tenderItem.Id
               where bidItem.TenderBidId == tenderBidId
               orderby bidItem.MescCode
               select new ContractItemSnapshot(tenderItem.PurchaseFileItemId, bidItem.Id, tenderItem.MescItemId,
                   bidItem.MescCode, bidItem.MescGeneralGroupCode, bidItem.GeneralDescription,
                   bidItem.SpecificDescription, tenderItem.UnitOfMeasureId, bidItem.Quantity, bidItem.UnitPrice,
                   null, bidItem.TechnicalNote))
            .ToListAsync(ct);
}
