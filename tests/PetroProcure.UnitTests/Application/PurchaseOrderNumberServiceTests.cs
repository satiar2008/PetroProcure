using PetroProcure.Application.PurchaseOrders;

namespace PetroProcure.UnitTests.Application;

public class PurchaseOrderNumberServiceTests
{
    [Fact]
    public async Task Generates_valid_purchase_order_number_format()
    {
        var service = new PurchaseOrderNumberService(new FakeRepository("PO-2026-000001"));
        var number = await service.GenerateNextPurchaseOrderNumber(2026);
        Assert.Equal("PO-2026-000001", number);
    }

    [Fact]
    public async Task Rejects_invalid_generated_number()
    {
        var service = new PurchaseOrderNumberService(new FakeRepository("PO-26-1"));
        await Assert.ThrowsAsync<PurchaseOrderValidationException>(() => service.GenerateNextPurchaseOrderNumber(2026));
    }

    private sealed class FakeRepository(string number) : IPurchaseOrderRepository
    {
        public Task<string> GenerateNextPurchaseOrderNumberAsync(int year, CancellationToken ct) => Task.FromResult(number);
        public Task<bool> PurchaseOrderNumberExistsAsync(string purchaseOrderNumber, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct) => Task.FromResult(false);
        public Task<PetroProcure.Domain.Modules.PurchaseOrders.PurchaseOrder?> FindPurchaseOrderAsync(Guid id, bool includeDetails, CancellationToken ct) => Task.FromResult<PetroProcure.Domain.Modules.PurchaseOrders.PurchaseOrder?>(null);
        public Task<ContractPurchaseOrderSnapshot?> GetContractSnapshotAsync(Guid contractId, CancellationToken ct) => Task.FromResult<ContractPurchaseOrderSnapshot?>(null);
        public Task<IReadOnlyList<PurchaseOrderItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PurchaseOrderItemSnapshot>>([]);
        public Task<PurchaseOrderItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct) => Task.FromResult<PurchaseOrderItemSnapshot?>(null);
        public Task<PurchaseOrderItemSnapshot?> GetContractItemSnapshotAsync(Guid contractItemId, CancellationToken ct) => Task.FromResult<PurchaseOrderItemSnapshot?>(null);
        public Task AddPurchaseOrderAsync(PetroProcure.Domain.Modules.PurchaseOrders.PurchaseOrder purchaseOrder, CancellationToken ct) => Task.CompletedTask;
        public Task AddPurchaseOrderDocumentAsync(PetroProcure.Domain.Modules.PurchaseOrders.PurchaseOrderDocument document, CancellationToken ct) => Task.CompletedTask;
        public Task<PetroProcure.Contracts.V1.Common.PagedResult<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>> GetPurchaseOrdersAsync(PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderListRequest request, CancellationToken ct) => throw new NotImplementedException();
        public Task<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(Guid id, CancellationToken ct) => Task.FromResult<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDetailDto?>(null);
        public Task<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDetailDto?> GetPurchaseOrderDetailByNumberAsync(string purchaseOrderNumber, CancellationToken ct) => Task.FromResult<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDetailDto?>(null);
        public Task<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>> GetPurchaseOrdersByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>>([]);
        public Task<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>> GetPurchaseOrdersBySupplierAsync(Guid supplierId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>>([]);
        public Task<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>> GetPurchaseOrdersByContractAsync(Guid contractId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderSummaryDto>>([]);
        public Task<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDocumentDto>> GetDocumentsAsync(Guid purchaseOrderId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PetroProcure.Contracts.V1.PurchaseOrders.PurchaseOrderDocumentDto>>([]);
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
