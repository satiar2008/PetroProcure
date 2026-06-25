using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;

namespace PetroProcure.UnitTests.Domain;

public sealed class ContractTests
{
    [Fact]
    public void Contract_cannot_be_submitted_without_items()
    {
        var contract = CreateContract();
        contract.AddClause(CreateRequiredClause(contract.Id));

        var exception = Assert.Throws<InvalidOperationException>(() => contract.Submit(Guid.NewGuid()));

        Assert.Contains("without at least one item", exception.Message);
    }

    [Fact]
    public void Contract_cannot_be_submitted_without_required_clauses()
    {
        var contract = CreateContract();
        contract.AddItem(CreateItem(contract.Id));

        var exception = Assert.Throws<InvalidOperationException>(() => contract.Submit(Guid.NewGuid()));

        Assert.Contains("required clauses", exception.Message);
    }

    [Fact]
    public void Contract_approval_changes_status()
    {
        var contract = ReadyContract();
        var userId = Guid.NewGuid();

        contract.Submit(userId);
        contract.Approve(userId);

        Assert.Equal(ContractStatus.Approved, contract.Status);
        Assert.Equal(userId, contract.ApprovedByUserId);
        Assert.NotNull(contract.ApprovedAt);
    }

    [Fact]
    public void Signed_contract_is_read_only()
    {
        var contract = ReadyContract();
        var userId = Guid.NewGuid();
        contract.Submit(userId);
        contract.Approve(userId);
        contract.Sign(userId);

        var exception = Assert.Throws<InvalidOperationException>(() => contract.AddClause(
            new ContractClause(Guid.NewGuid(), contract.Id, 2, "Extra", "Extra body",
                ContractClauseType.General, false, true, userId)));

        Assert.Contains("read-only", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Contract_item_keeps_mesc_snapshot()
    {
        var contract = CreateContract();

        contract.AddItem(CreateItem(contract.Id));

        var item = Assert.Single(contract.Items);
        Assert.Equal("123456", item.MescGeneralGroupCode);
        Assert.Equal("Pumps", item.GeneralDescription);
        Assert.Equal("Centrifugal pump", item.SpecificDescription);
    }

    private static PurchaseContract ReadyContract()
    {
        var contract = CreateContract();
        contract.AddItem(CreateItem(contract.Id));
        contract.AddClause(CreateRequiredClause(contract.Id));
        return contract;
    }

    private static PurchaseContract CreateContract() => new(
        Guid.NewGuid(), "CNT-2026-000001", Guid.NewGuid(), Guid.NewGuid(),
        null, null, null, null, "Pump contract", "Supply pumps",
        ContractType.DirectPurchase, "IRR", Guid.NewGuid());

    private static PurchaseContractItem CreateItem(Guid contractId) => new(
        Guid.NewGuid(), contractId, Guid.NewGuid(), null, Guid.NewGuid(),
        "1234560001", "123456", "Pumps", "Centrifugal pump",
        Guid.NewGuid(), 2, 100, null, "API pump");

    private static ContractClause CreateRequiredClause(Guid contractId) => new(
        Guid.NewGuid(), contractId, 1, "Subject", "Supplier shall deliver the goods.",
        ContractClauseType.General, true, true, Guid.NewGuid());
}
