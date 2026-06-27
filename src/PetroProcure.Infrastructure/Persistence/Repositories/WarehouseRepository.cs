using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Warehouse;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseOrders;
using PetroProcure.Contracts.V1.Warehouse;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Orders;
using PetroProcure.Domain.Modules.Warehouse;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class WarehouseRepository(PetroProcureDbContext db) : IWarehouseRepository
{
    public Task<string> GenerateNextReceiptNumberAsync(int year, CancellationToken ct) =>
        Next(year, db.WarehouseReceiptSequences, y => new WarehouseReceiptSequence(Guid.NewGuid(), y, 1), "WR", ct);

    public Task<string> GenerateNextInventoryTransactionNumberAsync(int year, CancellationToken ct) =>
        Next(year, db.InventoryTransactionSequences, y => new InventoryTransactionSequence(Guid.NewGuid(), y, 1), "ITX", ct);

    private async Task<string> Next<T>(int year, DbSet<T> set, Func<int, T> create, string prefix, CancellationToken ct)
        where T : class
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            dynamic? sequence = await set.SingleOrDefaultAsync(x => EF.Property<int>(x, "Year") == year, ct);
            int next;
            if (sequence is null)
            {
                next = 1;
                set.Add(create(year));
            }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return $"{prefix}-{year:0000}-{next:000000}";
        });
    }

    public async Task AddWarehouseAsync(Domain.Modules.Warehouse.Warehouse warehouse, CancellationToken ct) =>
        await db.Warehouses.AddAsync(warehouse, ct);
    public Task<Domain.Modules.Warehouse.Warehouse?> FindWarehouseAsync(Guid id, CancellationToken ct) =>
        db.Warehouses.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> WarehouseCodeExistsAsync(string code, Guid? excludingId, CancellationToken ct) =>
        db.Warehouses.AnyAsync(x => x.Code == code && (!excludingId.HasValue || x.Id != excludingId), ct);

    public async Task<PurchaseOrderReceiptSnapshot?> GetPurchaseOrderReceiptSnapshotAsync(Guid purchaseOrderId, CancellationToken ct)
    {
        var po = await (from order in db.PurchaseOrders.AsNoTracking()
                        join file in db.PurchaseFiles.AsNoTracking() on order.PurchaseFileId equals file.Id
                        join supplier in db.Suppliers.AsNoTracking() on order.SupplierId equals supplier.Id
                        where order.Id == purchaseOrderId
                        select new { order.Id, order.PurchaseOrderNumber, order.PurchaseFileId, file.FileNumber, order.SupplierId, SupplierName = supplier.Name, order.Status })
            .SingleOrDefaultAsync(ct);
        if (po is null) return null;
        var items = await db.PurchaseOrderItems.AsNoTracking().Where(x => x.PurchaseOrderId == purchaseOrderId)
            .OrderBy(x => x.MescCode)
            .Select(x => new PurchaseOrderItemReceiptSnapshot(x.Id, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.OrderedQuantity, x.ReceivedQuantity, x.RemainingQuantity))
            .ToListAsync(ct);
        return new PurchaseOrderReceiptSnapshot(po.Id, po.PurchaseOrderNumber, po.PurchaseFileId,
            po.FileNumber, po.SupplierId, po.SupplierName, po.Status, items);
    }

    public async Task AddReceiptAsync(WarehouseReceipt receipt, CancellationToken ct) => await db.WarehouseReceipts.AddAsync(receipt, ct);
    public Task<WarehouseReceipt?> FindReceiptAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<WarehouseReceipt> query = db.WarehouseReceipts;
        if (includeDetails) query = query.Include(x => x.Items).Include(x => x.Documents);
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }
    public async Task AddReceiptDocumentAsync(WarehouseReceiptDocument document, CancellationToken ct) =>
        await db.WarehouseReceiptDocuments.AddAsync(document, ct);

    public async Task ApplyReceiptToPurchaseOrderAndStockAsync(WarehouseReceipt receipt, IReadOnlyList<WarehouseReceiptItem> items,
        Func<Task<string>> transactionNumberFactory, Guid userId, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.Include(x => x.Items).SingleAsync(x => x.Id == receipt.PurchaseOrderId, ct);
        foreach (var item in items)
        {
            order.ApplyReceipt(item.PurchaseOrderItemId, item.ReceivedQuantity);
            var balance = await db.StockBalances.SingleOrDefaultAsync(x => x.MescItemId == item.MescItemId && x.WarehouseId == receipt.WarehouseId, ct);
            if (balance is null)
            {
                balance = new StockBalance(Guid.NewGuid(), item.MescItemId, receipt.WarehouseId, item.ReceivedQuantity, 0, 0);
                db.StockBalances.Add(balance);
            }
            else
            {
                balance.SetQuantities(balance.AvailableQuantity + item.ReceivedQuantity, balance.ReservedQuantity, balance.OnOrderQuantity);
            }
            db.InventoryTransactions.Add(new InventoryTransaction(Guid.NewGuid(), await transactionNumberFactory(),
                item.MescItemId, receipt.WarehouseId, InventoryTransactionType.Receipt,
                InventoryTransactionReferenceType.WarehouseReceipt, receipt.Id, item.ReceivedQuantity,
                item.UnitOfMeasureId, receipt.ReceiptDate, userId, $"رسید انبار {receipt.ReceiptNumber}"));
        }
    }

    public async Task<PagedResult<WarehouseDto>> GetWarehousesAsync(WarehouseListRequest r, CancellationToken ct)
    {
        var query = db.Warehouses.AsNoTracking().AsQueryable();
        if (!r.IncludeInactive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(r.SearchTerm))
        {
            var term = r.SearchTerm.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.Code).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new WarehouseDto(x.Id, x.Code, x.Name, x.Location, x.IsActive, x.Description)).ToListAsync(ct);
        return new PagedResult<WarehouseDto>(items, r.PageNumber, r.PageSize, total);
    }

    public Task<WarehouseDto?> GetWarehouseAsync(Guid id, CancellationToken ct) =>
        db.Warehouses.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new WarehouseDto(x.Id, x.Code, x.Name, x.Location, x.IsActive, x.Description))
            .SingleOrDefaultAsync(ct);

    public async Task<PagedResult<WarehouseReceiptSummaryDto>> GetReceiptsAsync(WarehouseReceiptListRequest r, CancellationToken ct)
    {
        var query = ApplyReceiptFilters(JoinReceipts(), r);
        var total = await query.LongCountAsync(ct);
        var ordered = r.SortBy?.ToLowerInvariant() switch
        {
            "receiptnumber" => r.SortDescending ? query.OrderByDescending(x => x.Receipt.ReceiptNumber) : query.OrderBy(x => x.Receipt.ReceiptNumber),
            "receiptdate" => r.SortDescending ? query.OrderByDescending(x => x.Receipt.ReceiptDate) : query.OrderBy(x => x.Receipt.ReceiptDate),
            "status" => r.SortDescending ? query.OrderByDescending(x => x.Receipt.Status) : query.OrderBy(x => x.Receipt.Status),
            _ => r.SortDescending ? query.OrderByDescending(x => x.Receipt.CreatedAt) : query.OrderBy(x => x.Receipt.CreatedAt)
        };
        var items = await Project(ordered.Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new PagedResult<WarehouseReceiptSummaryDto>(items, r.PageNumber, r.PageSize, total);
    }

    public async Task<WarehouseReceiptDetailDto?> GetReceiptDetailAsync(Guid id, CancellationToken ct)
    {
        var receipt = await DtoQuery(id: id).SingleOrDefaultAsync(ct);
        return receipt is null ? null : await Detail(receipt, ct);
    }
    public async Task<WarehouseReceiptDetailDto?> GetReceiptDetailByNumberAsync(string receiptNumber, CancellationToken ct)
    {
        var receipt = await DtoQuery(number: receiptNumber).SingleOrDefaultAsync(ct);
        return receipt is null ? null : await Detail(receipt, ct);
    }
    public async Task<IReadOnlyList<WarehouseReceiptSummaryDto>> GetReceiptsByPurchaseOrderAsync(Guid purchaseOrderId, CancellationToken ct) =>
        await Project(JoinReceipts().Where(x => x.Receipt.PurchaseOrderId == purchaseOrderId).OrderByDescending(x => x.Receipt.CreatedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<WarehouseReceiptSummaryDto>> GetReceiptsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        await Project(JoinReceipts().Where(x => x.Receipt.PurchaseFileId == purchaseFileId).OrderByDescending(x => x.Receipt.CreatedAt)).ToListAsync(ct);

    public async Task<IReadOnlyList<WarehouseReceiptDocumentDto>> GetDocumentsAsync(Guid receiptId, CancellationToken ct) =>
        await db.WarehouseReceiptDocuments.AsNoTracking().Where(x => x.WarehouseReceiptId == receiptId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new WarehouseReceiptDocumentDto(x.Id, x.WarehouseReceiptId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId))
            .ToListAsync(ct);

    public async Task<PagedResult<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionListRequest r, CancellationToken ct)
    {
        // Filter/order on entity columns first, then project to the DTO last so EF can
        // translate the ORDER BY (ordering over a positional-record projection across joins fails).
        var query = from tx in db.InventoryTransactions.AsNoTracking()
                    join item in db.MescItems.AsNoTracking() on tx.MescItemId equals item.Id
                    join wh in db.Warehouses.AsNoTracking() on tx.WarehouseId equals wh.Id
                    select new { tx, item, wh };
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.tx.TransactionNumber.Contains(r.SearchTerm) || x.item.Code.Contains(r.SearchTerm));
        if (r.WarehouseId.HasValue) query = query.Where(x => x.tx.WarehouseId == r.WarehouseId);
        if (r.TransactionType.HasValue) query = query.Where(x => x.tx.TransactionType == r.TransactionType);
        if (r.DateFrom.HasValue) query = query.Where(x => x.tx.TransactionDate >= r.DateFrom);
        if (r.DateTo.HasValue) query = query.Where(x => x.tx.TransactionDate <= r.DateTo);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderByDescending(x => x.tx.TransactionDate).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new InventoryTransactionDto(x.tx.Id, x.tx.TransactionNumber, x.tx.MescItemId, x.item.Code,
                x.item.Description, x.tx.WarehouseId, x.wh.Name, x.tx.TransactionType, x.tx.ReferenceType,
                x.tx.ReferenceId, x.tx.Quantity, x.tx.UnitOfMeasureId, x.tx.TransactionDate, x.tx.CreatedAt,
                x.tx.CreatedByUserId, x.tx.Description))
            .ToListAsync(ct);
        return new PagedResult<InventoryTransactionDto>(items, r.PageNumber, r.PageSize, total);
    }

    public async Task<PagedResult<StockBalanceDto>> GetStockBalancesAsync(StockBalanceListRequest r, CancellationToken ct)
    {
        // Keep the joined entities (no DTO projection yet) so filtering and ordering run on
        // real columns; the StockBalanceDto projection is applied last. Ordering over a
        // positional-record projection across these joins cannot be translated by EF Core.
        var query = from balance in db.StockBalances.AsNoTracking()
                    join item in db.MescItems.AsNoTracking() on balance.MescItemId equals item.Id
                    join groupItem in db.MescGeneralGroups.AsNoTracking() on item.GeneralGroupCode equals groupItem.Code
                    join wh in db.Warehouses.AsNoTracking() on balance.WarehouseId equals wh.Id into warehouses
                    from wh in warehouses.DefaultIfEmpty()
                    select new { balance, item, groupItem, wh };
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.item.Code.Contains(r.SearchTerm) || x.item.Description.Contains(r.SearchTerm));
        if (r.WarehouseId.HasValue) query = query.Where(x => x.balance.WarehouseId == r.WarehouseId);
        if (!string.IsNullOrWhiteSpace(r.MescGeneralGroupCode)) query = query.Where(x => x.item.GeneralGroupCode == r.MescGeneralGroupCode);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.item.Code).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new StockBalanceDto(x.balance.Id, x.balance.MescItemId, x.balance.WarehouseId, x.item.Code,
                x.item.GeneralGroupCode, x.groupItem.Description, x.item.Description,
                x.wh == null ? null : x.wh.Name, x.balance.AvailableQuantity, x.balance.ReservedQuantity,
                x.balance.OnOrderQuantity, x.balance.LastUpdatedAt))
            .ToListAsync(ct);
        return new PagedResult<StockBalanceDto>(items, r.PageNumber, r.PageSize, total);
    }

    public async Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetIssuedPurchaseOrdersWaitingForReceiptAsync(CancellationToken ct)
    {
        var rows = await db.PurchaseOrders.AsNoTracking()
            .Where(x => x.Status == PurchaseOrderStatus.Issued || x.Status == PurchaseOrderStatus.PartiallyReceived)
            .Join(db.PurchaseFiles.AsNoTracking(), x => x.PurchaseFileId, f => f.Id, (x, f) => new { Order = x, File = f })
            .Join(db.Suppliers.AsNoTracking(), x => x.Order.SupplierId, s => s.Id, (x, supplier) => new
            {
                x.Order.Id,
                x.Order.PurchaseOrderNumber,
                x.Order.PurchaseFileId,
                PurchaseFileNumber = x.File.FileNumber,
                x.Order.SupplierId,
                SupplierName = supplier.Name,
                x.Order.ContractId,
                x.Order.Title,
                x.Order.PurchaseOrderType,
                x.Order.Status,
                x.Order.FinalAmount,
                x.Order.Currency,
                x.Order.OrderDate,
                x.Order.ExpectedDeliveryDate,
                x.Order.CreatedAt
            })
            .OrderBy(x => x.ExpectedDeliveryDate)
            .ToListAsync(ct);

        return rows.Select(x => new PurchaseOrderSummaryDto(
            x.Id, x.PurchaseOrderNumber, x.PurchaseFileId, x.PurchaseFileNumber, x.SupplierId,
            x.SupplierName, x.ContractId, null, x.Title, x.PurchaseOrderType, x.Status, x.FinalAmount,
            x.Currency, x.OrderDate, x.ExpectedDeliveryDate, x.CreatedAt)).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    // Join receipts to their related entities WITHOUT projecting into the DTO yet.
    // Filtering and ordering are done on the underlying entity columns (via ReceiptJoin.Receipt),
    // and the DTO projection is applied last. Ordering directly over a positional-record
    // projection across these joins cannot be translated by EF Core.
    private IQueryable<ReceiptJoin> JoinReceipts() =>
        from r in db.WarehouseReceipts.AsNoTracking()
        join po in db.PurchaseOrders.AsNoTracking() on r.PurchaseOrderId equals po.Id
        join pf in db.PurchaseFiles.AsNoTracking() on r.PurchaseFileId equals pf.Id
        join wh in db.Warehouses.AsNoTracking() on r.WarehouseId equals wh.Id
        join supplier in db.Suppliers.AsNoTracking() on r.SupplierId equals supplier.Id
        select new ReceiptJoin
        {
            Receipt = r,
            PurchaseOrderNumber = po.PurchaseOrderNumber,
            PurchaseFileNumber = pf.FileNumber,
            WarehouseName = wh.Name,
            SupplierName = supplier.Name
        };

    private static IQueryable<WarehouseReceiptSummaryDto> Project(IQueryable<ReceiptJoin> query) =>
        query.Select(j => new WarehouseReceiptSummaryDto(j.Receipt.Id, j.Receipt.ReceiptNumber,
            j.Receipt.PurchaseOrderId, j.PurchaseOrderNumber, j.Receipt.PurchaseFileId, j.PurchaseFileNumber,
            j.Receipt.WarehouseId, j.WarehouseName, j.Receipt.SupplierId, j.SupplierName,
            j.Receipt.Status, j.Receipt.ReceiptDate, j.Receipt.CreatedAt));

    private sealed class ReceiptJoin
    {
        public required WarehouseReceipt Receipt { get; init; }
        public required string PurchaseOrderNumber { get; init; }
        public required string PurchaseFileNumber { get; init; }
        public required string WarehouseName { get; init; }
        public required string SupplierName { get; init; }
    }

    private IQueryable<WarehouseReceiptDto> DtoQuery(Guid? id = null, string? number = null)
    {
        var receipts = db.WarehouseReceipts.AsNoTracking();
        if (id.HasValue) receipts = receipts.Where(x => x.Id == id);
        if (!string.IsNullOrWhiteSpace(number)) receipts = receipts.Where(x => x.ReceiptNumber == number);
        return from r in receipts
               join po in db.PurchaseOrders.AsNoTracking() on r.PurchaseOrderId equals po.Id
               join pf in db.PurchaseFiles.AsNoTracking() on r.PurchaseFileId equals pf.Id
               join wh in db.Warehouses.AsNoTracking() on r.WarehouseId equals wh.Id
               join supplier in db.Suppliers.AsNoTracking() on r.SupplierId equals supplier.Id
               select new WarehouseReceiptDto(r.Id, r.ReceiptNumber, r.PurchaseOrderId, po.PurchaseOrderNumber,
                   r.PurchaseFileId, pf.FileNumber, r.WarehouseId, wh.Name, r.SupplierId, supplier.Name,
                   r.Status, r.ReceiptDate, r.DeliveryNoteNumber, r.CarrierName, r.VehicleNumber, r.ReceivedByUserId,
                   r.Description, r.CreatedAt, r.CreatedByUserId, r.ApprovedAt, r.ApprovedByUserId,
                   r.CancelledAt, r.CancelledByUserId, r.CancellationReason);
    }

    private async Task<WarehouseReceiptDetailDto> Detail(WarehouseReceiptDto receipt, CancellationToken ct)
    {
        var items = await db.WarehouseReceiptItems.AsNoTracking().Where(x => x.WarehouseReceiptId == receipt.Id)
            .OrderBy(x => x.MescCode)
            .Select(x => new WarehouseReceiptItemDto(x.Id, x.WarehouseReceiptId, x.PurchaseOrderItemId, x.MescItemId,
                x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
                x.UnitOfMeasureId, x.OrderedQuantity, x.PreviouslyReceivedQuantity, x.ReceivedQuantity,
                x.AcceptedQuantity, x.RejectedQuantity, x.RemainingQuantityAfterReceipt, x.BatchNumber,
                x.SerialNumber, x.ExpiryDate, x.QualityStatus, x.Notes)).ToListAsync(ct);
        var documents = await GetDocumentsAsync(receipt.Id, ct);
        var transactions = (await GetInventoryTransactionsAsync(new InventoryTransactionListRequest(PageSize: 200), ct)).Items
            .Where(x => x.ReferenceType == InventoryTransactionReferenceType.WarehouseReceipt && x.ReferenceId == receipt.Id).ToList();
        return new WarehouseReceiptDetailDto(receipt, items, documents, transactions);
    }

    private static IQueryable<ReceiptJoin> ApplyReceiptFilters(IQueryable<ReceiptJoin> query, WarehouseReceiptListRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.Receipt.ReceiptNumber.Contains(r.SearchTerm) || x.PurchaseOrderNumber.Contains(r.SearchTerm) || x.PurchaseFileNumber.Contains(r.SearchTerm) || x.SupplierName.Contains(r.SearchTerm));
        if (r.Status.HasValue) query = query.Where(x => x.Receipt.Status == r.Status);
        if (!string.IsNullOrWhiteSpace(r.ReceiptNumber)) query = query.Where(x => x.Receipt.ReceiptNumber.Contains(r.ReceiptNumber));
        if (!string.IsNullOrWhiteSpace(r.PurchaseOrderNumber)) query = query.Where(x => x.PurchaseOrderNumber.Contains(r.PurchaseOrderNumber));
        if (!string.IsNullOrWhiteSpace(r.PurchaseFileNumber)) query = query.Where(x => x.PurchaseFileNumber.Contains(r.PurchaseFileNumber));
        if (r.SupplierId.HasValue) query = query.Where(x => x.Receipt.SupplierId == r.SupplierId);
        if (r.WarehouseId.HasValue) query = query.Where(x => x.Receipt.WarehouseId == r.WarehouseId);
        if (r.ReceiptDateFrom.HasValue) query = query.Where(x => x.Receipt.ReceiptDate >= r.ReceiptDateFrom);
        if (r.ReceiptDateTo.HasValue) query = query.Where(x => x.Receipt.ReceiptDate <= r.ReceiptDateTo);
        return query;
    }
}
