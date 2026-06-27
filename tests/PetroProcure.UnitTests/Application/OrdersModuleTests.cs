using PetroProcure.Application.Indents;
using PetroProcure.Application.Orders;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Orders;

namespace PetroProcure.UnitTests.Application;

public sealed class OrdersModuleTests
{
    [Fact]
    public async Task Approved_needs_from_same_group_are_combined_into_one_indent()
    {
        var departmentId = Guid.NewGuid();
        var repo = new FakeOrdersRepository
        {
            Needs =
            [
                ApprovedNeed("MN-2026-000001", departmentId, "123456", "1234560001", 2),
                ApprovedNeed("MN-2026-000002", departmentId, "123456", "1234560002", 3)
            ]
        };
        var handler = new OrdersCommandHandler(repo, new FakeIndentNumberService(), new TestCurrentUser(Guid.NewGuid(), [departmentId]));

        var indentId = await handler.Handle(new ConvertMaterialNeedsToIndentCommand(
            repo.Needs.Select(x => x.Id).ToArray(), 26, 3, null));

        var indent = Assert.Single(repo.Indents);
        Assert.Equal(indent.Id, indentId);
        Assert.Equal(2, indent.Items.Count);
        Assert.All(repo.Needs, x =>
        {
            Assert.Equal(MaterialNeedStatus.ConvertedToIndent, x.Status);
            Assert.Equal(indent.Id, x.RelatedIndentId);
        });
    }

    [Fact]
    public async Task Combining_needs_from_different_groups_is_rejected()
    {
        var departmentId = Guid.NewGuid();
        var repo = new FakeOrdersRepository
        {
            Needs =
            [
                ApprovedNeed("MN-2026-000001", departmentId, "123456", "1234560001", 2),
                ApprovedNeed("MN-2026-000002", departmentId, "654321", "6543210001", 3)
            ]
        };
        var handler = new OrdersCommandHandler(repo, new FakeIndentNumberService(), new TestCurrentUser(Guid.NewGuid(), [departmentId]));

        await Assert.ThrowsAsync<OrdersValidationException>(() => handler.Handle(new ConvertMaterialNeedsToIndentCommand(
            repo.Needs.Select(x => x.Id).ToArray(), 26, 3, null)));
    }

    private static MaterialNeed ApprovedNeed(string number, Guid departmentId, string groupCode, string mescCode, decimal quantity)
    {
        var need = new MaterialNeed(Guid.NewGuid(), number, Guid.NewGuid(), mescCode, groupCode,
            $"Group {groupCode}", $"Item {mescCode}", Guid.NewGuid(), quantity, null,
            departmentId, null, Guid.NewGuid(), MaterialNeedPriority.Normal, "Need");
        need.Submit();
        need.Approve(Guid.NewGuid());
        return need;
    }

    private sealed class FakeIndentNumberService : IIndentNumberService
    {
        public Task<string> GenerateNextIndentNumber(int yearPart, int typeDigit, CancellationToken cancellationToken = default) =>
            Task.FromResult($"{yearPart:00}{typeDigit}0001");
        public bool ValidateIndentNumber(string indentNumber) => true;
        public IndentNumberParts ParseIndentNumber(string indentNumber) => new(indentNumber, 26, 3, 1, IndentType.Manual);
        public IndentType ResolveIndentType(int typeDigit) => IndentType.Manual;
    }

    private sealed class FakeOrdersRepository : IOrdersRepository
    {
        public List<MaterialNeed> Needs { get; set; } = [];
        public List<Indent> Indents { get; } = [];
        public Task<string> GenerateNextNeedNumberAsync(int year, CancellationToken ct) => Task.FromResult($"MN-{year:0000}-000001");
        public Task<MescOrderSnapshot?> GetMescSnapshotAsync(Guid mescItemId, CancellationToken ct) => Task.FromResult<MescOrderSnapshot?>(null);
        public Task<Guid> ResolveUnitOfMeasureIdAsync(string unitOfMeasure, CancellationToken ct) => Task.FromResult(Guid.NewGuid());
        public Task<InventoryControlItem?> FindInventoryControlItemAsync(Guid id, CancellationToken ct) => Task.FromResult<InventoryControlItem?>(null);
        public Task<InventoryControlItem?> FindInventoryControlItemByMescItemAsync(Guid mescItemId, CancellationToken ct) => Task.FromResult<InventoryControlItem?>(null);
        public Task<MaterialNeed?> FindMaterialNeedAsync(Guid id, CancellationToken ct) => Task.FromResult(Needs.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<MaterialNeed>> FindMaterialNeedsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<MaterialNeed>>(Needs.Where(x => ids.Contains(x.Id)).ToArray());
        public Task<ShortageAlert?> FindShortageAlertAsync(Guid id, CancellationToken ct) => Task.FromResult<ShortageAlert?>(null);
        public Task AddMaterialNeedAsync(MaterialNeed need, CancellationToken ct) { Needs.Add(need); return Task.CompletedTask; }
        public Task AddInventoryControlItemAsync(InventoryControlItem item, CancellationToken ct) => Task.CompletedTask;
        public Task AddShortageAlertAsync(ShortageAlert alert, CancellationToken ct) => Task.CompletedTask;
        public Task AddIndentAsync(Indent indent, CancellationToken ct) { Indents.Add(indent); return Task.CompletedTask; }
        public Task ApplyStockAdjustmentAsync(InventoryControlItem item, Guid warehouseId, decimal quantity,
            Func<Task<string>> transactionNumberFactory, Guid userId, string? description, CancellationToken ct) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
        public Task<PagedResult<InventoryControlItemDto>> GetInventoryControlItemsAsync(InventoryControlListRequest request, CancellationToken ct) =>
            Task.FromResult(new PagedResult<InventoryControlItemDto>([], request.PageNumber, request.PageSize, 0));
        public Task<PagedResult<StockBalanceDto>> GetStockBalancesAsync(StockBalanceListRequest request, CancellationToken ct) =>
            Task.FromResult(new PagedResult<StockBalanceDto>([], request.PageNumber, request.PageSize, 0));
        public Task<PagedResult<MaterialNeedDto>> GetMaterialNeedsAsync(MaterialNeedListRequest request, CancellationToken ct) =>
            Task.FromResult(new PagedResult<MaterialNeedDto>([], request.PageNumber, request.PageSize, 0));
        public Task<MaterialNeedDto?> GetMaterialNeedDtoAsync(Guid id, CancellationToken ct) => Task.FromResult<MaterialNeedDto?>(null);
        public Task<PagedResult<ShortageAlertDto>> GetShortageAlertsAsync(ShortageAlertListRequest request, CancellationToken ct) =>
            Task.FromResult(new PagedResult<ShortageAlertDto>([], request.PageNumber, request.PageSize, 0));
        public Task<IReadOnlyList<MaterialNeedsGroupedByMescDto>> GetMaterialNeedsGroupedByMescAsync(MaterialNeedListRequest request, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<MaterialNeedsGroupedByMescDto>>([]);
        public Task<OrdersDashboardDto> GetDashboardAsync(CancellationToken ct) => Task.FromResult(new OrdersDashboardDto(0, 0, 0, 0, 0, [], []));
        public Task<IReadOnlyList<ShortageAlert>> DetectShortagesAsync(bool includeExistingOpen, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ShortageAlert>>([]);
    }
}
