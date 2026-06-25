using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.PurchaseOrders;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseOrders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseOrders;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(PetroProcureDbContext db) : IPurchaseOrderRepository
{
    public async Task<string> GenerateNextPurchaseOrderNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.PurchaseOrderSequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null)
            {
                next = 1;
                db.PurchaseOrderSequences.Add(new PurchaseOrderSequence(Guid.NewGuid(), year, next));
            }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return $"PO-{year:0000}-{next:000000}";
        });
    }

    public Task<bool> PurchaseOrderNumberExistsAsync(string purchaseOrderNumber, CancellationToken ct) =>
        db.PurchaseOrders.AnyAsync(x => x.PurchaseOrderNumber == purchaseOrderNumber, ct);

    public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) =>
        db.PurchaseFiles.AnyAsync(x => x.Id == purchaseFileId, ct);

    public Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct) =>
        db.Suppliers.AnyAsync(x => x.Id == supplierId && x.IsActive && !x.IsBlacklisted, ct);

    public Task<PurchaseOrder?> FindPurchaseOrderAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<PurchaseOrder> query = db.PurchaseOrders;
        if (includeDetails) query = query.Include(x => x.Items).Include(x => x.Approvals).Include(x => x.Documents);
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<ContractPurchaseOrderSnapshot?> GetContractSnapshotAsync(Guid contractId, CancellationToken ct)
    {
        var contract = await db.PurchaseContracts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == contractId, ct);
        if (contract is null) return null;
        var items = await db.PurchaseContractItems.AsNoTracking()
            .Where(x => x.ContractId == contractId)
            .OrderBy(x => x.MescCode)
            .Select(x => new PurchaseOrderItemSnapshot(x.PurchaseFileItemId, x.Id, x.TenderBidItemId,
                x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription,
                x.SpecificDescription, x.UnitOfMeasureId, x.Quantity, x.UnitPrice, x.DeliveryDate,
                x.TechnicalDescription, null))
            .ToListAsync(ct);
        return new ContractPurchaseOrderSnapshot(contract.Id, contract.ContractNumber, contract.PurchaseFileId,
            contract.SupplierId, contract.TenderId, contract.TenderBidId, contract.Title, contract.Status,
            contract.Currency, contract.TotalAmount, contract.TaxAmount, contract.FinalAmount,
            contract.DeliveryDeadline, contract.DeliveryTerms, contract.PaymentTerms, contract.WarrantyTerms, items);
    }

    public async Task<IReadOnlyList<PurchaseOrderItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct) =>
        await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderBy(x => x.MescCode)
            .Select(x => new PurchaseOrderItemSnapshot(x.Id, null, null, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, null, null, x.TechnicalDescription, null))
            .ToListAsync(ct);

    public Task<PurchaseOrderItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct) =>
        db.PurchaseFileItems.AsNoTracking().Where(x => x.Id == purchaseFileItemId)
            .Select(x => new PurchaseOrderItemSnapshot(x.Id, null, null, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, null, null, x.TechnicalDescription, null))
            .SingleOrDefaultAsync(ct);

    public Task<PurchaseOrderItemSnapshot?> GetContractItemSnapshotAsync(Guid contractItemId, CancellationToken ct) =>
        db.PurchaseContractItems.AsNoTracking().Where(x => x.Id == contractItemId)
            .Select(x => new PurchaseOrderItemSnapshot(x.PurchaseFileItemId, x.Id, x.TenderBidItemId,
                x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
                x.UnitOfMeasureId, x.Quantity, x.UnitPrice, x.DeliveryDate, x.TechnicalDescription, null))
            .SingleOrDefaultAsync(ct);

    public async Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder, CancellationToken ct) =>
        await db.PurchaseOrders.AddAsync(purchaseOrder, ct);

    public async Task AddPurchaseOrderDocumentAsync(PurchaseOrderDocument document, CancellationToken ct) =>
        await db.PurchaseOrderDocuments.AddAsync(document, ct);

    public async Task<PagedResult<PurchaseOrderSummaryDto>> GetPurchaseOrdersAsync(PurchaseOrderListRequest request, CancellationToken ct)
    {
        var query = ApplyFilters(SummaryQuery(), request);
        var total = await query.LongCountAsync(ct);
        var rows = await ApplySort(query, request.SortBy, request.SortDescending)
            .Skip((Math.Max(1, request.PageNumber) - 1) * Math.Max(1, request.PageSize))
            .Take(Math.Clamp(request.PageSize, 1, 200))
            .ToListAsync(ct);
        var items = rows.Select(x => x.ToDto()).ToArray();
        return new PagedResult<PurchaseOrderSummaryDto>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(Guid id, CancellationToken ct)
    {
        var po = await DtoQuery(id: id).SingleOrDefaultAsync(ct);
        return po is null ? null : await Detail(po, ct);
    }

    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailByNumberAsync(string purchaseOrderNumber, CancellationToken ct)
    {
        var po = await DtoQuery(number: purchaseOrderNumber).SingleOrDefaultAsync(ct);
        return po is null ? null : await Detail(po, ct);
    }

    public async Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        (await SummaryQuery().Where(x => x.PurchaseFileId == purchaseFileId).OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct)).Select(x => x.ToDto()).ToArray();

    public async Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersBySupplierAsync(Guid supplierId, CancellationToken ct) =>
        (await SummaryQuery().Where(x => x.SupplierId == supplierId).OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct)).Select(x => x.ToDto()).ToArray();

    public async Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersByContractAsync(Guid contractId, CancellationToken ct) =>
        (await SummaryQuery().Where(x => x.ContractId == contractId).OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct)).Select(x => x.ToDto()).ToArray();

    public async Task<IReadOnlyList<PurchaseOrderDocumentDto>> GetDocumentsAsync(Guid purchaseOrderId, CancellationToken ct) =>
        await db.PurchaseOrderDocuments.AsNoTracking().Where(x => x.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new PurchaseOrderDocumentDto(x.Id, x.PurchaseOrderId, x.FileDocumentId,
                x.DocumentType, x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private async Task<PurchaseOrderDetailDto> Detail(PurchaseOrderDto po, CancellationToken ct)
    {
        var items = await db.PurchaseOrderItems.AsNoTracking().Where(x => x.PurchaseOrderId == po.Id)
            .OrderBy(x => x.MescGeneralGroupCode).ThenBy(x => x.MescCode)
            .Select(x => new PurchaseOrderItemDto(x.Id, x.PurchaseOrderId, x.PurchaseFileItemId,
                x.ContractItemId, x.TenderBidItemId, x.MescItemId, x.MescCode, x.MescGeneralGroupCode,
                x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.OrderedQuantity,
                x.ReceivedQuantity, x.RemainingQuantity, x.UnitPrice, x.TotalPrice, x.ExpectedDeliveryDate,
                x.TechnicalDescription, x.Notes))
            .ToListAsync(ct);
        var approvals = await db.PurchaseOrderApprovals.AsNoTracking().Where(x => x.PurchaseOrderId == po.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseOrderApprovalDto(x.Id, x.PurchaseOrderId, x.ApprovalStep,
                x.DepartmentId, x.ApproverUserId, x.Status, x.Comment, x.CreatedAt, x.ApprovedAt, x.RejectedAt))
            .ToListAsync(ct);
        var documents = await GetDocumentsAsync(po.Id, ct);
        return new PurchaseOrderDetailDto(po, items, approvals, documents);
    }

    private IQueryable<PurchaseOrderSummaryRow> SummaryQuery() =>
        from po in db.PurchaseOrders.AsNoTracking()
        join file in db.PurchaseFiles.AsNoTracking() on po.PurchaseFileId equals file.Id
        join supplier in db.Suppliers.AsNoTracking() on po.SupplierId equals supplier.Id
        join contract in db.PurchaseContracts.AsNoTracking() on po.ContractId equals contract.Id into contracts
        from contract in contracts.DefaultIfEmpty()
        select new PurchaseOrderSummaryRow
        {
            Id = po.Id,
            PurchaseOrderNumber = po.PurchaseOrderNumber,
            PurchaseFileId = po.PurchaseFileId,
            PurchaseFileNumber = file.FileNumber,
            SupplierId = po.SupplierId,
            SupplierName = supplier.Name,
            ContractId = po.ContractId,
            ContractNumber = contract == null ? null : contract.ContractNumber,
            Title = po.Title,
            PurchaseOrderType = po.PurchaseOrderType,
            Status = po.Status,
            FinalAmount = po.FinalAmount,
            Currency = po.Currency,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            CreatedAt = po.CreatedAt
        };

    private IQueryable<PurchaseOrderDto> DtoQuery(Guid? id = null, string? number = null)
    {
        var orders = db.PurchaseOrders.AsNoTracking();
        if (id.HasValue) orders = orders.Where(x => x.Id == id.Value);
        if (!string.IsNullOrWhiteSpace(number)) orders = orders.Where(x => x.PurchaseOrderNumber == number);

        return from po in orders
               join file in db.PurchaseFiles.AsNoTracking() on po.PurchaseFileId equals file.Id
               join supplier in db.Suppliers.AsNoTracking() on po.SupplierId equals supplier.Id
               join contract in db.PurchaseContracts.AsNoTracking() on po.ContractId equals contract.Id into contracts
               from contract in contracts.DefaultIfEmpty()
               join tender in db.Tenders.AsNoTracking() on po.TenderId equals tender.Id into tenders
               from tender in tenders.DefaultIfEmpty()
               select new PurchaseOrderDto(po.Id, po.PurchaseOrderNumber, po.PurchaseFileId, file.FileNumber,
                   po.SupplierId, supplier.Name, po.ContractId, contract == null ? null : contract.ContractNumber,
                   po.TenderId, tender == null ? null : tender.TenderNumber, po.TenderBidId, po.Title,
                   po.Description, po.Status, po.PurchaseOrderType, po.Currency, po.TotalAmount, po.TaxAmount,
                   po.DiscountAmount, po.FinalAmount, po.OrderDate, po.ExpectedDeliveryDate, po.DeliveryLocation,
                   po.DeliveryTerms, po.PaymentTerms, po.WarrantyTerms, po.Notes, po.CreatedAt,
                   po.CreatedByUserId, po.SubmittedAt, po.SubmittedByUserId, po.ApprovedAt, po.ApprovedByUserId,
                   po.IssuedAt, po.IssuedByUserId, po.CompletedAt, po.CompletedByUserId,
                   po.CancelledAt, po.CancelledByUserId, po.CancellationReason);
    }

    private static IQueryable<PurchaseOrderSummaryRow> ApplyFilters(IQueryable<PurchaseOrderSummaryRow> query, PurchaseOrderListRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.SearchTerm))
        {
            var term = r.SearchTerm.Trim();
            query = query.Where(x => x.PurchaseOrderNumber.Contains(term) || x.Title.Contains(term)
                || x.SupplierName.Contains(term) || x.PurchaseFileNumber.Contains(term)
                || (x.ContractNumber != null && x.ContractNumber.Contains(term)));
        }
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        if (r.PurchaseOrderType.HasValue) query = query.Where(x => x.PurchaseOrderType == r.PurchaseOrderType);
        if (r.SupplierId.HasValue) query = query.Where(x => x.SupplierId == r.SupplierId);
        if (!string.IsNullOrWhiteSpace(r.ContractNumber)) query = query.Where(x => x.ContractNumber != null && x.ContractNumber.Contains(r.ContractNumber));
        if (!string.IsNullOrWhiteSpace(r.PurchaseFileNumber)) query = query.Where(x => x.PurchaseFileNumber.Contains(r.PurchaseFileNumber));
        if (r.DateFrom.HasValue) query = query.Where(x => x.CreatedAt >= r.DateFrom.Value);
        if (r.DateTo.HasValue) query = query.Where(x => x.CreatedAt <= r.DateTo.Value);
        return query;
    }

    private static IQueryable<PurchaseOrderSummaryRow> ApplySort(IQueryable<PurchaseOrderSummaryRow> query, string? sortBy, bool desc) =>
        (sortBy ?? "CreatedAt").ToLowerInvariant() switch
        {
            "purchaseordernumber" => desc ? query.OrderByDescending(x => x.PurchaseOrderNumber) : query.OrderBy(x => x.PurchaseOrderNumber),
            "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "orderdate" => desc ? query.OrderByDescending(x => x.OrderDate) : query.OrderBy(x => x.OrderDate),
            "expecteddeliverydate" => desc ? query.OrderByDescending(x => x.ExpectedDeliveryDate) : query.OrderBy(x => x.ExpectedDeliveryDate),
            "finalamount" => desc ? query.OrderByDescending(x => x.FinalAmount) : query.OrderBy(x => x.FinalAmount),
            _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

    private sealed class PurchaseOrderSummaryRow
    {
        public Guid Id { get; init; }
        public string PurchaseOrderNumber { get; init; } = string.Empty;
        public Guid PurchaseFileId { get; init; }
        public string PurchaseFileNumber { get; init; } = string.Empty;
        public Guid SupplierId { get; init; }
        public string SupplierName { get; init; } = string.Empty;
        public Guid? ContractId { get; init; }
        public string? ContractNumber { get; init; }
        public string Title { get; init; } = string.Empty;
        public PurchaseOrderType PurchaseOrderType { get; init; }
        public PurchaseOrderStatus Status { get; init; }
        public decimal? FinalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime? OrderDate { get; init; }
        public DateTime? ExpectedDeliveryDate { get; init; }
        public DateTime CreatedAt { get; init; }

        public PurchaseOrderSummaryDto ToDto() => new(Id, PurchaseOrderNumber, PurchaseFileId,
            PurchaseFileNumber, SupplierId, SupplierName, ContractId, ContractNumber, Title,
            PurchaseOrderType, Status, FinalAmount, Currency, OrderDate, ExpectedDeliveryDate, CreatedAt);
    }
}
