using Microsoft.Extensions.Options;
using PetroProcure.Application.Mesc;
using PetroProcure.Domain.Modules.Items;

namespace PetroProcure.UnitTests.Application;

public sealed class MescCatalogTests
{
    [Fact]
    public async Task CreatingItem_DerivesFirstSixDigitGeneralGroupCode()
    {
        var repository = new FakeRepository();
        repository.Groups.Add(new MescGeneralGroup(Guid.NewGuid(), "123456", "Pipe fittings"));
        var handler = CreateCommandHandler(repository);

        var item = await handler.Handle(new CreateMescItemCommand("1234567890", "Steel pipe", "M"));

        Assert.Equal("123456", item.GeneralGroupCode);
        Assert.Equal("Pipe fittings", item.GeneralDescription);
    }

    [Fact]
    public async Task CreatingItem_FailsWhenGeneralGroupDoesNotExist()
    {
        var handler = CreateCommandHandler(new FakeRepository());

        var exception = await Assert.ThrowsAsync<MescCatalogValidationException>(() =>
            handler.Handle(new CreateMescItemCommand("9999990001", "Unknown item", "EA")));

        Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GroupedQuery_ReturnsItemsUnderGeneralDescription()
    {
        var repository = new FakeRepository();
        repository.ItemDtos.AddRange(
            new MescItemDto(Guid.NewGuid(), "1234560001", "123456", "Pipe fittings", "Pipe", "M", Guid.NewGuid(), true),
            new MescItemDto(Guid.NewGuid(), "1234560002", "123456", "Pipe fittings", "Elbow", "EA", Guid.NewGuid(), true));

        var result = await new MescQueryHandler(repository)
            .Handle(new GetMescItemsGroupedByGeneralCodeQuery());

        var group = Assert.Single(result);
        Assert.Equal("Pipe fittings", group.GeneralDescription);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public async Task Search_ExcludesDeactivatedItemsUnlessRequested()
    {
        var repository = new FakeRepository();
        repository.ItemDtos.AddRange(
            new MescItemDto(Guid.NewGuid(), "1234560001", "123456", "Pipe fittings", "Active pipe", "M", Guid.NewGuid(), true),
            new MescItemDto(Guid.NewGuid(), "1234560002", "123456", "Pipe fittings", "Inactive pipe", "M", Guid.NewGuid(), false));
        var handler = new MescQueryHandler(repository);

        var defaultResult = await handler.Handle(new SearchMescItemsQuery("pipe"));
        var withInactive = await handler.Handle(new SearchMescItemsQuery("pipe", true));

        Assert.Single(defaultResult);
        Assert.Equal(2, withInactive.Count);
    }

    private static MescCommandHandler CreateCommandHandler(FakeRepository repository) =>
        new(repository, Options.Create(new MescCatalogOptions()));

    private sealed class FakeRepository : IMescCatalogRepository
    {
        public List<MescGeneralGroup> Groups { get; } = [];
        public List<MescItem> Items { get; } = [];
        public List<MescItemDto> ItemDtos { get; } = [];

        public Task<bool> GeneralGroupCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
            Task.FromResult(Groups.Any(group => group.Code == code && group.Id != excludingId));
        public Task<bool> ItemCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(item => item.Code == code && item.Id != excludingId));
        public Task<MescGeneralGroup?> FindGeneralGroupAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Groups.SingleOrDefault(group => group.Id == id));
        public Task<MescGeneralGroup?> FindGeneralGroupByCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(Groups.SingleOrDefault(group => group.Code == code));
        public Task<MescItem?> FindItemAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item => item.Id == id));
        public Task<Guid> ResolveUnitOfMeasureIdAsync(string unitOfMeasure, CancellationToken cancellationToken) =>
            Task.FromResult(Guid.Parse("20000000-0000-0000-0000-000000000001"));
        public Task AddGeneralGroupAsync(MescGeneralGroup group, CancellationToken cancellationToken)
        {
            Groups.Add(group);
            return Task.CompletedTask;
        }
        public Task AddItemAsync(MescItem item, CancellationToken cancellationToken)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<MescGeneralGroupDto>> GetGeneralGroupsAsync(bool includeInactive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MescGeneralGroupDto>>(Groups
                .Where(group => includeInactive || group.IsActive)
                .Select(group => new MescGeneralGroupDto(group.Id, group.Code, group.Description, group.IsActive)).ToArray());
        public Task<IReadOnlyList<MescItemDto>> GetItemsAsync(bool includeInactive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MescItemDto>>(ItemDtos.Where(item => includeInactive || item.IsActive).ToArray());
        public Task<MescItemDto?> GetItemByCodeAsync(string code, bool includeInactive, CancellationToken cancellationToken) =>
            Task.FromResult(ItemDtos.SingleOrDefault(item => item.Code == code && (includeInactive || item.IsActive)));
        public Task<IReadOnlyList<MescItemDto>> SearchItemsAsync(string term, bool includeInactive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MescItemDto>>(ItemDtos.Where(item =>
                (includeInactive || item.IsActive) &&
                (item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 item.GeneralDescription.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 item.SpecificDescription.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray());
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
