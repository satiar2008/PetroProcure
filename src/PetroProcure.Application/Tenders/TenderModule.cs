using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Application.Tenders;

public sealed record CreateTenderCommand(Guid PurchaseFileId, string Title, TenderType TenderType, DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description);
public sealed record CreateTenderFromPurchaseFileCommand(Guid PurchaseFileId, string Title, TenderType TenderType, DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description, Guid[] PurchaseFileItemIds, Guid[] SupplierIds);
public sealed record CreateTenderFromInquiryCommand(Guid InquiryId, string Title, TenderType TenderType, DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description, Guid[] InquirySupplierIds);
public sealed record UpdateTenderCommand(Guid Id, string Title, TenderType TenderType, DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description);
public sealed record AddTenderItemCommand(Guid TenderId, Guid PurchaseFileItemId);
public sealed record RemoveTenderItemCommand(Guid TenderId, Guid ItemId);
public sealed record AddTenderParticipantCommand(Guid TenderId, Guid SupplierId, Guid? ContactId);
public sealed record RemoveTenderParticipantCommand(Guid TenderId, Guid ParticipantId);
public sealed record PublishTenderCommand(Guid TenderId);
public sealed record CancelTenderCommand(Guid TenderId, string Reason);
public sealed record AddTenderBidCommand(Guid TenderId, Guid TenderParticipantId, string? BidNumber, string? Currency, decimal? TotalAmount, decimal? FinalAmount, string? DeliveryTerms, string? PaymentTerms, DateTime? ValidUntil, string? Notes);
public sealed record UpdateTenderBidCommand(Guid TenderId, Guid BidId, string? BidNumber, string? Currency, decimal? TotalAmount, decimal? FinalAmount, string? DeliveryTerms, string? PaymentTerms, DateTime? ValidUntil, string? Notes);
public sealed record AddTenderBidItemCommand(Guid TenderId, Guid BidId, Guid TenderItemId, decimal Quantity, decimal? UnitPrice, TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);
public sealed record AddTenderEvaluationCommand(Guid TenderId, Guid TenderBidId, TenderEvaluationType EvaluationType, decimal? Score, TenderEvaluationResult Result, string? Notes);
public sealed record SelectTenderWinnerCommand(Guid TenderId, Guid TenderBidId, string? Reason, string? Notes);
public sealed record CloseTenderCommand(Guid TenderId);

public sealed record GetTendersQuery(TenderListRequest Request);
public sealed record GetTenderByIdQuery(Guid Id);
public sealed record GetTenderByNumberQuery(string TenderNumber);
public sealed record GetTendersByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetTenderItemsGroupedByMescQuery(Guid TenderId);
public sealed record GetTenderParticipantsQuery(Guid TenderId);
public sealed record GetTenderBidsQuery(Guid TenderId);
public sealed record GetTenderComparisonQuery(Guid TenderId);
public sealed record GetTenderLookupQuery(string? Term);

public sealed record TenderPurchaseFileItemSnapshot(Guid Id, Guid PurchaseFileId, Guid MescItemId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal Quantity, string? TechnicalDescription);
public sealed record TenderSupplierSnapshot(Guid Id, string SupplierCode, string Name, bool IsActive, bool IsBlacklisted);
public sealed record TenderSupplierContactSnapshot(Guid Id, string FullName, string? Email);
public sealed record TenderInquirySnapshot(Guid Id, Guid PurchaseFileId, string InquiryNumber);
public sealed record TenderInquirySupplierSnapshot(Guid Id, Guid SupplierId, Guid? ContactId);

public interface ITenderRepository
{
    Task<string> GenerateNextTenderNumberAsync(int year, CancellationToken cancellationToken);
    Task<Tender?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken);
    Task<Tender?> FindByNumberAsync(string tenderNumber, CancellationToken cancellationToken);
    Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<TenderInquirySnapshot?> GetInquirySnapshotAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderInquirySupplierSnapshot>> GetInquirySuppliersAsync(Guid inquiryId, Guid[] inquirySupplierIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderPurchaseFileItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, Guid[] itemIds, CancellationToken cancellationToken);
    Task<TenderPurchaseFileItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileId, Guid itemId, CancellationToken cancellationToken);
    Task<TenderSupplierSnapshot?> GetSupplierSnapshotAsync(Guid supplierId, CancellationToken cancellationToken);
    Task<TenderSupplierContactSnapshot?> GetSupplierContactSnapshotAsync(Guid supplierId, Guid contactId, CancellationToken cancellationToken);
    Task AddAsync(Tender tender, CancellationToken cancellationToken);
    Task<PagedResult<TenderSummaryDto>> GetPagedAsync(TenderListRequest request, CancellationToken cancellationToken);
    Task<TenderDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<TenderDetailDto?> GetDetailByNumberAsync(string tenderNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderSummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderItemsGroupedDto>> GetItemsGroupedAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderParticipantDto>> GetParticipantsAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderBidDto>> GetBidsAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<TenderComparisonDto?> GetComparisonAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderLookupDto>> GetLookupAsync(string? term, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface ITenderNumberService
{
    Task<string> GenerateNextTenderNumber(int year, CancellationToken cancellationToken = default);
}

public sealed class TenderNumberService(ITenderRepository repository) : ITenderNumberService
{
    public Task<string> GenerateNextTenderNumber(int year, CancellationToken cancellationToken = default) =>
        repository.GenerateNextTenderNumberAsync(year, cancellationToken);
}

public interface ITenderEligibilityService
{
    void EnsureSupplierEligible(TenderSupplierSnapshot supplier);
}

public sealed class TenderEligibilityService : ITenderEligibilityService
{
    public void EnsureSupplierEligible(TenderSupplierSnapshot supplier)
    {
        if (!supplier.IsActive || supplier.IsBlacklisted)
            throw new TenderValidationException("Blacklisted or inactive suppliers cannot be added to tender.");
    }
}

public interface ITenderComparisonService;
public sealed class TenderComparisonService : ITenderComparisonService;

public sealed class TenderCommandHandler(ITenderRepository repository, ITenderNumberService numbers,
    ITenderEligibilityService eligibility, ICurrentUserService currentUser)
{
    public async Task<TenderDetailDto> Handle(CreateTenderCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        if (!await repository.PurchaseFileExistsAsync(command.PurchaseFileId, ct))
            throw new TenderValidationException("Purchase file was not found.");
        var number = await numbers.GenerateNextTenderNumber(DateTime.UtcNow.Year, ct);
        var tender = new Tender(Guid.NewGuid(), number, command.PurchaseFileId, null, command.Title, command.TenderType,
            DateTime.UtcNow, command.SubmissionDeadline, command.OpeningDate, command.Description, currentUser.UserId);
        await repository.AddAsync(tender, ct);
        await repository.SaveChangesAsync(ct);
        return await Detail(tender.Id, ct);
    }

    public async Task<TenderDetailDto> Handle(CreateTenderFromPurchaseFileCommand command, CancellationToken ct = default)
    {
        var detail = await Handle(new CreateTenderCommand(command.PurchaseFileId, command.Title, command.TenderType,
            command.SubmissionDeadline, command.OpeningDate, command.Description), ct);
        foreach (var item in await repository.GetPurchaseFileItemSnapshotsAsync(command.PurchaseFileId, command.PurchaseFileItemIds, ct))
            await AddItem(detail.Tender.Id, item, ct);
        foreach (var supplierId in command.SupplierIds.Distinct())
            await Handle(new AddTenderParticipantCommand(detail.Tender.Id, supplierId, null), ct);
        await repository.SaveChangesAsync(ct);
        return await Detail(detail.Tender.Id, ct);
    }

    public async Task<TenderDetailDto> Handle(CreateTenderFromInquiryCommand command, CancellationToken ct = default)
    {
        var inquiry = await repository.GetInquirySnapshotAsync(command.InquiryId, ct)
            ?? throw new TenderValidationException("Inquiry was not found.");
        var number = await numbers.GenerateNextTenderNumber(DateTime.UtcNow.Year, ct);
        var tender = new Tender(Guid.NewGuid(), number, inquiry.PurchaseFileId, inquiry.Id, command.Title, command.TenderType,
            DateTime.UtcNow, command.SubmissionDeadline, command.OpeningDate, command.Description, currentUser.UserId);
        await repository.AddAsync(tender, ct);
        foreach (var item in await repository.GetPurchaseFileItemSnapshotsAsync(inquiry.PurchaseFileId, [], ct))
            tender.AddItem(ToTenderItem(tender.Id, item));
        foreach (var supplier in await repository.GetInquirySuppliersAsync(inquiry.Id, command.InquirySupplierIds, ct))
            await AddParticipant(tender, supplier.SupplierId, supplier.ContactId, ct);
        await repository.SaveChangesAsync(ct);
        return await Detail(tender.Id, ct);
    }

    public async Task<TenderDetailDto> Handle(UpdateTenderCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.Id, true, ct);
        tender.Update(command.Title, command.TenderType, command.SubmissionDeadline, command.OpeningDate, command.Description);
        await repository.SaveChangesAsync(ct);
        return await Detail(tender.Id, ct);
    }

    public async Task<TenderItemDto> Handle(AddTenderItemCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        var item = await repository.GetPurchaseFileItemSnapshotAsync(tender.PurchaseFileId, command.PurchaseFileItemId, ct)
            ?? throw new TenderValidationException("Purchase file item was not found.");
        var entity = await AddItem(tender.Id, item, ct);
        await repository.SaveChangesAsync(ct);
        return (await Detail(tender.Id, ct)).Items.Single(x => x.Id == entity.Id);
    }

    public async Task Handle(RemoveTenderItemCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        tender.RemoveItem(command.ItemId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<TenderParticipantDto> Handle(AddTenderParticipantCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        var entity = await AddParticipant(tender, command.SupplierId, command.ContactId, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetParticipantsAsync(tender.Id, ct)).Single(x => x.Id == entity.Id);
    }

    public async Task Handle(RemoveTenderParticipantCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        tender.RemoveParticipant(command.ParticipantId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(PublishTenderCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        tender.Publish(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CancelTenderCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        tender.Cancel(command.Reason, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<TenderBidDto> Handle(AddTenderBidCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        var participant = tender.Participants.SingleOrDefault(x => x.Id == command.TenderParticipantId)
            ?? throw new TenderValidationException("Tender participant was not found.");
        var bid = new TenderBid(Guid.NewGuid(), tender.Id, participant.Id, participant.SupplierId, command.BidNumber,
            command.Currency, command.TotalAmount, command.FinalAmount, command.DeliveryTerms, command.PaymentTerms,
            command.ValidUntil, command.Notes, currentUser.UserId);
        participant.MarkSubmitted();
        tender.AddBid(bid);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetBidsAsync(tender.Id, ct)).Single(x => x.Id == bid.Id);
    }

    public async Task<TenderBidDto> Handle(UpdateTenderBidCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        var bid = tender.Bids.SingleOrDefault(x => x.Id == command.BidId) ?? throw new TenderValidationException("Tender bid was not found.");
        bid.Update(command.BidNumber, command.Currency, command.TotalAmount, command.FinalAmount, command.DeliveryTerms, command.PaymentTerms, command.ValidUntil, command.Notes);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetBidsAsync(tender.Id, ct)).Single(x => x.Id == bid.Id);
    }

    public async Task<TenderBidItemDto> Handle(AddTenderBidItemCommand command, CancellationToken ct = default)
    {
        var tender = await Find(command.TenderId, true, ct);
        var bid = tender.Bids.SingleOrDefault(x => x.Id == command.BidId) ?? throw new TenderValidationException("Tender bid was not found.");
        var item = tender.Items.SingleOrDefault(x => x.Id == command.TenderItemId) ?? throw new TenderValidationException("Tender item was not found.");
        var entity = new TenderBidItem(Guid.NewGuid(), bid.Id, item.Id, item.MescCode, item.MescGeneralGroupCode,
            item.GeneralDescription, item.SpecificDescription, command.Quantity, command.UnitPrice,
            command.TechnicalComplianceStatus, command.TechnicalNote);
        bid.AddItem(entity);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetBidsAsync(tender.Id, ct)).SelectMany(x => x.Items).Single(x => x.Id == entity.Id);
    }

    public async Task<TenderEvaluationDto> Handle(AddTenderEvaluationCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        var bid = tender.Bids.SingleOrDefault(x => x.Id == command.TenderBidId) ?? throw new TenderValidationException("Tender bid was not found.");
        var evaluation = new TenderEvaluation(Guid.NewGuid(), tender.Id, bid.Id, command.EvaluationType, currentUser.UserId,
            command.Score, command.Result, command.Notes);
        bid.ApplyEvaluation(command.EvaluationType, command.Score);
        tender.AddEvaluation(evaluation);
        await repository.SaveChangesAsync(ct);
        return (await Detail(tender.Id, ct)).Evaluations.Single(x => x.Id == evaluation.Id);
    }

    public async Task Handle(SelectTenderWinnerCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        tender.SelectWinner(command.TenderBidId, currentUser.UserId, command.Reason, command.Notes);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CloseTenderCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await Find(command.TenderId, true, ct);
        tender.Close(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    private async Task<TenderItem> AddItem(Guid tenderId, TenderPurchaseFileItemSnapshot item, CancellationToken ct)
    {
        var tender = await Find(tenderId, true, ct);
        var entity = ToTenderItem(tender.Id, item);
        tender.AddItem(entity);
        return entity;
    }

    private async Task<TenderParticipant> AddParticipant(Tender tender, Guid supplierId, Guid? contactId, CancellationToken ct)
    {
        var supplier = await repository.GetSupplierSnapshotAsync(supplierId, ct) ?? throw new TenderValidationException("Supplier was not found.");
        eligibility.EnsureSupplierEligible(supplier);
        TenderSupplierContactSnapshot? contact = null;
        if (contactId.HasValue) contact = await repository.GetSupplierContactSnapshotAsync(supplierId, contactId.Value, ct);
        var entity = new TenderParticipant(Guid.NewGuid(), tender.Id, supplier.Id, supplier.SupplierCode, supplier.Name,
            contact?.Id, contact?.FullName, contact?.Email);
        tender.AddParticipant(entity);
        return entity;
    }

    private static TenderItem ToTenderItem(Guid tenderId, TenderPurchaseFileItemSnapshot item) =>
        new(Guid.NewGuid(), tenderId, item.Id, item.MescItemId, item.MescCode, item.MescGeneralGroupCode,
            item.GeneralDescription, item.SpecificDescription, item.UnitOfMeasureId, item.Quantity, item.TechnicalDescription);

    private async Task<Tender> Find(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindAsync(id, includeDetails, ct) ?? throw new TenderNotFoundException("Tender was not found.");
    private async Task<TenderDetailDto> Detail(Guid id, CancellationToken ct) =>
        await repository.GetDetailAsync(id, ct) ?? throw new TenderNotFoundException("Tender was not found.");
    private void EnsureUser() { if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty) throw new CurrentUserNotAuthenticatedException(); }
}

public sealed class TenderQueryHandler(ITenderRepository repository)
{
    public Task<PagedResult<TenderSummaryDto>> Handle(GetTendersQuery query, CancellationToken ct = default) => repository.GetPagedAsync(query.Request, ct);
    public Task<TenderDetailDto?> Handle(GetTenderByIdQuery query, CancellationToken ct = default) => repository.GetDetailAsync(query.Id, ct);
    public Task<TenderDetailDto?> Handle(GetTenderByNumberQuery query, CancellationToken ct = default) => repository.GetDetailByNumberAsync(query.TenderNumber, ct);
    public Task<IReadOnlyList<TenderSummaryDto>> Handle(GetTendersByPurchaseFileQuery query, CancellationToken ct = default) => repository.GetByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<IReadOnlyList<TenderItemsGroupedDto>> Handle(GetTenderItemsGroupedByMescQuery query, CancellationToken ct = default) => repository.GetItemsGroupedAsync(query.TenderId, ct);
    public Task<IReadOnlyList<TenderParticipantDto>> Handle(GetTenderParticipantsQuery query, CancellationToken ct = default) => repository.GetParticipantsAsync(query.TenderId, ct);
    public Task<IReadOnlyList<TenderBidDto>> Handle(GetTenderBidsQuery query, CancellationToken ct = default) => repository.GetBidsAsync(query.TenderId, ct);
    public Task<TenderComparisonDto?> Handle(GetTenderComparisonQuery query, CancellationToken ct = default) => repository.GetComparisonAsync(query.TenderId, ct);
    public Task<IReadOnlyList<TenderLookupDto>> Handle(GetTenderLookupQuery query, CancellationToken ct = default) => repository.GetLookupAsync(query.Term, ct);
}

public sealed class TenderValidationException(string message) : Exception(message);
public sealed class TenderNotFoundException(string message) : Exception(message);
