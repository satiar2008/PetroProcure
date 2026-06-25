using PetroProcure.Application.Contracts;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Contracts;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;

namespace PetroProcure.UnitTests.Application;

public sealed class ContractModuleTests
{
    [Fact]
    public async Task Contract_number_is_generated_correctly()
    {
        var service = new ContractNumberService(new FakeRepository());

        var number = await service.GenerateNextContractNumber(2026);

        Assert.Equal("CNT-2026-000001", number);
    }

    [Fact]
    public async Task Invalid_contract_year_is_rejected()
    {
        var service = new ContractNumberService(new FakeRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GenerateNextContractNumber(99));
    }

    [Fact]
    public async Task Applying_template_copies_clauses_to_contract()
    {
        var template = new ContractTemplate(Guid.NewGuid(), "DIRECT", "Direct template",
            null, ContractType.DirectPurchase, Guid.NewGuid());
        template.AddClause(new ContractTemplateClause(Guid.NewGuid(), template.Id, 1,
            "Subject", "Contract subject clause", ContractClauseType.General, true, true));
        var repository = new FakeRepository { Template = template };
        var contract = new PurchaseContract(Guid.NewGuid(), "CNT-2026-000001",
            Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, "Contract",
            "Subject", ContractType.DirectPurchase, "IRR", Guid.NewGuid());

        await new ContractTemplateService(repository).ApplyTemplateAsync(contract, template.Id, Guid.NewGuid());

        var clause = Assert.Single(contract.Clauses);
        Assert.Equal("Subject", clause.Title);
        Assert.True(clause.IsRequired);
        Assert.Equal(template.Id, contract.ContractTemplateId);
    }

    private sealed class FakeRepository : IContractRepository
    {
        private readonly Dictionary<int, int> _sequences = [];
        public ContractTemplate? Template { get; set; }

        public Task<string> GenerateNextContractNumberAsync(int year, CancellationToken ct)
        {
            var next = _sequences.GetValueOrDefault(year) + 1;
            _sequences[year] = next;
            return Task.FromResult($"CNT-{year:0000}-{next:000000}");
        }

        public Task<ContractTemplate?> FindTemplateAsync(Guid id, bool includeClauses, CancellationToken ct) =>
            Task.FromResult(Template?.Id == id ? Template : null);

        public Task<bool> ContractNumberExistsAsync(string contractNumber, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct) => Task.FromResult(true);
        public Task<PurchaseContract?> FindContractAsync(Guid id, bool includeDetails, CancellationToken ct) => Task.FromResult<PurchaseContract?>(null);
        public Task<ContractClause?> FindClauseAsync(Guid contractId, Guid clauseId, CancellationToken ct) => Task.FromResult<ContractClause?>(null);
        public Task<ContractTemplateClause?> FindTemplateClauseAsync(Guid templateId, Guid clauseId, CancellationToken ct) => Task.FromResult<ContractTemplateClause?>(null);
        public Task<bool> TemplateCodeExistsAsync(string templateCode, Guid? excludingId, CancellationToken ct) => Task.FromResult(false);
        public Task<IReadOnlyList<ContractItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult<IReadOnlyList<ContractItemSnapshot>>([]);
        public Task<ContractItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct) => Task.FromResult<ContractItemSnapshot?>(null);
        public Task<TenderContractSnapshot?> GetTenderSnapshotAsync(Guid tenderId, Guid? supplierId, Guid? tenderBidId, CancellationToken ct) => Task.FromResult<TenderContractSnapshot?>(null);
        public Task<TenderContractSnapshot?> GetTenderBidSnapshotAsync(Guid tenderBidId, CancellationToken ct) => Task.FromResult<TenderContractSnapshot?>(null);
        public Task AddContractAsync(PurchaseContract contract, CancellationToken ct) => Task.CompletedTask;
        public Task AddTemplateAsync(ContractTemplate template, CancellationToken ct) => Task.CompletedTask;
        public Task AddContractDocumentAsync(ContractDocument document, CancellationToken ct) => Task.CompletedTask;
        public Task<PagedResult<PurchaseContractSummaryDto>> GetContractsAsync(ContractListRequest request, CancellationToken ct) =>
            Task.FromResult(new PagedResult<PurchaseContractSummaryDto>([], request.PageNumber, request.PageSize, 0));
        public Task<PurchaseContractDetailDto?> GetContractDetailAsync(Guid id, CancellationToken ct) => Task.FromResult<PurchaseContractDetailDto?>(null);
        public Task<PurchaseContractDetailDto?> GetContractDetailByNumberAsync(string contractNumber, CancellationToken ct) => Task.FromResult<PurchaseContractDetailDto?>(null);
        public Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PurchaseContractSummaryDto>>([]);
        public Task<IReadOnlyList<PurchaseContractSummaryDto>> GetContractsBySupplierAsync(Guid supplierId, CancellationToken ct) => Task.FromResult<IReadOnlyList<PurchaseContractSummaryDto>>([]);
        public Task<IReadOnlyList<ContractTemplateDto>> GetTemplatesAsync(bool includeInactive, CancellationToken ct) => Task.FromResult<IReadOnlyList<ContractTemplateDto>>([]);
        public Task<ContractTemplateDto?> GetTemplateAsync(Guid id, CancellationToken ct) => Task.FromResult<ContractTemplateDto?>(null);
        public Task<IReadOnlyList<ContractDocumentDto>> GetDocumentsAsync(Guid contractId, CancellationToken ct) => Task.FromResult<IReadOnlyList<ContractDocumentDto>>([]);
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
