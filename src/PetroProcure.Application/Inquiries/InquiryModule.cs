using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Inquiry;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Inquiries;

namespace PetroProcure.Application.Inquiries;

public sealed record CreateInquiryCommand(Guid PurchaseFileId, string Title, InquiryType InquiryType, DateTime? DeadlineDate, string? Description);
public sealed record CreateInquiryFromPurchaseFileCommand(Guid PurchaseFileId, string Title, InquiryType InquiryType, DateTime? DeadlineDate, string? Description, Guid[] PurchaseFileItemIds, Guid[] SupplierIds);
public sealed record UpdateInquiryCommand(Guid Id, string Title, InquiryType InquiryType, DateTime? DeadlineDate, string? Description);
public sealed record AddInquiryItemCommand(Guid InquiryId, Guid PurchaseFileItemId);
public sealed record RemoveInquiryItemCommand(Guid InquiryId, Guid ItemId);
public sealed record AddInquirySupplierCommand(Guid InquiryId, Guid SupplierId, Guid? ContactId);
public sealed record RemoveInquirySupplierCommand(Guid InquiryId, Guid InquirySupplierId);
public sealed record SendInquiryCommand(Guid InquiryId);
public sealed record CancelInquiryCommand(Guid InquiryId, string Reason);
public sealed record CloseInquiryCommand(Guid InquiryId);
public sealed record AddSupplierQuoteCommand(Guid InquiryId, Guid InquirySupplierId, string? QuoteNumber, DateTime? QuoteDate, DateTime? ValidUntil, string Currency, string? DeliveryTerms, string? PaymentTerms, DateTime? DeliveryDate, decimal TotalAmount, decimal? TaxAmount, decimal? DiscountAmount, string? TechnicalNote, string? CommercialNote);
public sealed record AddSupplierQuoteItemCommand(Guid InquiryId, Guid QuoteId, Guid InquiryItemId, decimal Quantity, decimal UnitPrice, DateTime? DeliveryDate, TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);
public sealed record SelectSupplierQuoteCommand(Guid InquiryId, Guid QuoteId, string? Reason);
public sealed record RejectSupplierQuoteCommand(Guid InquiryId, Guid QuoteId, string? Reason);

public sealed record GetInquiriesQuery(InquiryListRequest Request);
public sealed record GetInquiryByIdQuery(Guid Id);
public sealed record GetInquiryByNumberQuery(string InquiryNumber);
public sealed record GetInquiriesByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetInquiryItemsGroupedByMescQuery(Guid InquiryId);
public sealed record GetInquirySuppliersQuery(Guid InquiryId);
public sealed record GetSupplierQuotesQuery(Guid InquiryId);
public sealed record GetInquiryComparisonQuery(Guid InquiryId);
public sealed record GetInquiryLookupQuery(string? Term);

public sealed record PurchaseFileItemInquirySnapshot(Guid Id, Guid PurchaseFileId, Guid MescItemId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal RequestedQuantity, string? TechnicalDescription);
public sealed record SupplierInquirySnapshot(Guid Id, string SupplierCode, string Name, bool IsActive, bool IsBlacklisted);
public sealed record SupplierContactInquirySnapshot(Guid Id, string FullName, string? Email);

public interface IInquiryRepository
{
    Task<string> GenerateNextInquiryNumberAsync(int year, CancellationToken cancellationToken);
    Task<bool> InquiryNumberExistsAsync(string inquiryNumber, CancellationToken cancellationToken);
    Task<Inquiry?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken);
    Task<Inquiry?> FindByNumberAsync(string inquiryNumber, CancellationToken cancellationToken);
    Task<PurchaseFileItemInquirySnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileId, Guid purchaseFileItemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseFileItemInquirySnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, Guid[] itemIds, CancellationToken cancellationToken);
    Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<SupplierInquirySnapshot?> GetSupplierSnapshotAsync(Guid supplierId, CancellationToken cancellationToken);
    Task<SupplierContactInquirySnapshot?> GetSupplierContactSnapshotAsync(Guid supplierId, Guid contactId, CancellationToken cancellationToken);
    Task AddAsync(Inquiry inquiry, CancellationToken cancellationToken);
    Task<PagedResult<InquirySummaryDto>> GetPagedAsync(InquiryListRequest request, CancellationToken cancellationToken);
    Task<InquiryDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<InquiryDetailDto?> GetDetailByNumberAsync(string inquiryNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<InquirySummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InquiryItemsGroupedDto>> GetItemsGroupedAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InquirySupplierDto>> GetSuppliersAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplierQuoteDto>> GetQuotesAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task<InquiryComparisonDto?> GetComparisonAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InquiryLookupDto>> GetLookupAsync(string? term, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed class InquiryNumberService(IInquiryRepository repository) : IInquiryNumberService
{
    public Task<string> GenerateNextInquiryNumber(int year, CancellationToken cancellationToken = default) =>
        repository.GenerateNextInquiryNumberAsync(year, cancellationToken);
}

public interface IInquiryNumberService
{
    Task<string> GenerateNextInquiryNumber(int year, CancellationToken cancellationToken = default);
}

public sealed class InquiryCommandHandler(IInquiryRepository repository, IInquiryNumberService numbers, ICurrentUserService currentUser)
{
    public async Task<InquiryDetailDto> Handle(CreateInquiryCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        if (!await repository.PurchaseFileExistsAsync(command.PurchaseFileId, ct)) throw new InquiryValidationException("Purchase file was not found.");
        var number = await numbers.GenerateNextInquiryNumber(DateTime.UtcNow.Year, ct);
        var inquiry = new Inquiry(Guid.NewGuid(), number, command.PurchaseFileId, command.Title, command.InquiryType, DateTime.UtcNow, command.DeadlineDate, command.Description, currentUser.UserId);
        await repository.AddAsync(inquiry, ct);
        await repository.SaveChangesAsync(ct);
        return await repository.GetDetailAsync(inquiry.Id, ct) ?? throw new InquiryNotFoundException("Inquiry was not found.");
    }

    public async Task<InquiryDetailDto> Handle(CreateInquiryFromPurchaseFileCommand command, CancellationToken ct = default)
    {
        var inquiry = await Handle(new CreateInquiryCommand(command.PurchaseFileId, command.Title, command.InquiryType, command.DeadlineDate, command.Description), ct);
        foreach (var item in await repository.GetPurchaseFileItemSnapshotsAsync(command.PurchaseFileId, command.PurchaseFileItemIds, ct))
            await AddItem(inquiry.Inquiry.Id, item, ct);
        foreach (var supplierId in command.SupplierIds.Distinct())
            await Handle(new AddInquirySupplierCommand(inquiry.Inquiry.Id, supplierId, null), ct);
        await repository.SaveChangesAsync(ct);
        return await repository.GetDetailAsync(inquiry.Inquiry.Id, ct) ?? throw new InquiryNotFoundException("Inquiry was not found.");
    }

    public async Task<InquiryDetailDto> Handle(UpdateInquiryCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.Id, true, ct);
        inquiry.Update(command.Title, command.InquiryType, command.DeadlineDate, command.Description);
        await repository.SaveChangesAsync(ct);
        return await repository.GetDetailAsync(inquiry.Id, ct) ?? throw new InquiryNotFoundException("Inquiry was not found.");
    }

    public async Task<InquiryItemDto> Handle(AddInquiryItemCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        var snapshot = await repository.GetPurchaseFileItemSnapshotAsync(inquiry.PurchaseFileId, command.PurchaseFileItemId, ct)
            ?? throw new InquiryValidationException("Purchase file item was not found.");
        var item = await AddItem(command.InquiryId, snapshot, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetDetailAsync(command.InquiryId, ct))!.Items.Single(x => x.Id == item.Id);
    }

    public async Task Handle(RemoveInquiryItemCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.RemoveItem(command.ItemId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<InquirySupplierDto> Handle(AddInquirySupplierCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        var supplier = await repository.GetSupplierSnapshotAsync(command.SupplierId, ct)
            ?? throw new InquiryValidationException("Supplier was not found.");
        if (!supplier.IsActive || supplier.IsBlacklisted)
            throw new InquiryValidationException("Blacklisted or inactive suppliers cannot be added to inquiry.");
        SupplierContactInquirySnapshot? contact = null;
        if (command.ContactId.HasValue)
            contact = await repository.GetSupplierContactSnapshotAsync(command.SupplierId, command.ContactId.Value, ct);
        var entity = new InquirySupplier(Guid.NewGuid(), inquiry.Id, supplier.Id, supplier.SupplierCode, supplier.Name,
            contact?.Id, contact?.FullName, contact?.Email);
        inquiry.AddSupplier(entity);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetSuppliersAsync(inquiry.Id, ct)).Single(x => x.Id == entity.Id);
    }

    public async Task Handle(RemoveInquirySupplierCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.RemoveSupplier(command.InquirySupplierId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(SendInquiryCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.Send(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CancelInquiryCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.Cancel(command.Reason, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CloseInquiryCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.Close(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<SupplierQuoteDto> Handle(AddSupplierQuoteCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var inquiry = await Find(command.InquiryId, true, ct);
        var inquirySupplier = inquiry.Suppliers.SingleOrDefault(x => x.Id == command.InquirySupplierId)
            ?? throw new InquiryValidationException("Inquiry supplier was not found.");
        var quote = new SupplierQuote(Guid.NewGuid(), inquiry.Id, inquirySupplier.SupplierId, inquirySupplier.Id,
            command.QuoteNumber, command.QuoteDate, command.ValidUntil, command.Currency, command.DeliveryTerms,
            command.PaymentTerms, command.DeliveryDate, command.TotalAmount, command.TaxAmount, command.DiscountAmount,
            command.TechnicalNote, command.CommercialNote, currentUser.UserId);
        inquirySupplier.MarkResponded();
        inquiry.AddQuote(quote);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetQuotesAsync(inquiry.Id, ct)).Single(x => x.Id == quote.Id);
    }

    public async Task<SupplierQuoteItemDto> Handle(AddSupplierQuoteItemCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        var quote = inquiry.Quotes.SingleOrDefault(x => x.Id == command.QuoteId) ?? throw new InquiryValidationException("Quote was not found.");
        var item = inquiry.Items.SingleOrDefault(x => x.Id == command.InquiryItemId) ?? throw new InquiryValidationException("Inquiry item was not found.");
        var quoteItem = new SupplierQuoteItem(Guid.NewGuid(), quote.Id, item.Id, item.MescCode, item.MescGeneralGroupCode,
            item.GeneralDescription, item.SpecificDescription, command.Quantity, command.UnitPrice, command.DeliveryDate,
            command.TechnicalComplianceStatus, command.TechnicalNote);
        quote.AddItem(quoteItem);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetQuotesAsync(inquiry.Id, ct)).SelectMany(x => x.Items).Single(x => x.Id == quoteItem.Id);
    }

    public async Task Handle(SelectSupplierQuoteCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        inquiry.SelectQuote(command.QuoteId, command.Reason);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(RejectSupplierQuoteCommand command, CancellationToken ct = default)
    {
        var inquiry = await Find(command.InquiryId, true, ct);
        var quote = inquiry.Quotes.SingleOrDefault(x => x.Id == command.QuoteId) ?? throw new InquiryValidationException("Quote was not found.");
        quote.Reject(command.Reason);
        await repository.SaveChangesAsync(ct);
    }

    private async Task<Inquiry> Find(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindAsync(id, includeDetails, ct) ?? throw new InquiryNotFoundException("Inquiry was not found.");
    private async Task<InquiryItem> AddItem(Guid inquiryId, PurchaseFileItemInquirySnapshot item, CancellationToken ct)
    {
        var inquiry = await Find(inquiryId, true, ct);
        var entity = new InquiryItem(Guid.NewGuid(), inquiry.Id, item.Id, item.MescItemId, item.MescCode,
            item.MescGeneralGroupCode, item.GeneralDescription, item.SpecificDescription, item.UnitOfMeasureId,
            item.RequestedQuantity, item.TechnicalDescription);
        inquiry.AddItem(entity);
        return entity;
    }
    private void EnsureUser() { if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty) throw new CurrentUserNotAuthenticatedException(); }
}

public sealed class InquiryQueryHandler(IInquiryRepository repository)
{
    public Task<PagedResult<InquirySummaryDto>> Handle(GetInquiriesQuery query, CancellationToken ct = default) => repository.GetPagedAsync(query.Request, ct);
    public Task<InquiryDetailDto?> Handle(GetInquiryByIdQuery query, CancellationToken ct = default) => repository.GetDetailAsync(query.Id, ct);
    public Task<InquiryDetailDto?> Handle(GetInquiryByNumberQuery query, CancellationToken ct = default) => repository.GetDetailByNumberAsync(query.InquiryNumber, ct);
    public Task<IReadOnlyList<InquirySummaryDto>> Handle(GetInquiriesByPurchaseFileQuery query, CancellationToken ct = default) => repository.GetByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<IReadOnlyList<InquiryItemsGroupedDto>> Handle(GetInquiryItemsGroupedByMescQuery query, CancellationToken ct = default) => repository.GetItemsGroupedAsync(query.InquiryId, ct);
    public Task<IReadOnlyList<InquirySupplierDto>> Handle(GetInquirySuppliersQuery query, CancellationToken ct = default) => repository.GetSuppliersAsync(query.InquiryId, ct);
    public Task<IReadOnlyList<SupplierQuoteDto>> Handle(GetSupplierQuotesQuery query, CancellationToken ct = default) => repository.GetQuotesAsync(query.InquiryId, ct);
    public Task<InquiryComparisonDto?> Handle(GetInquiryComparisonQuery query, CancellationToken ct = default) => repository.GetComparisonAsync(query.InquiryId, ct);
    public Task<IReadOnlyList<InquiryLookupDto>> Handle(GetInquiryLookupQuery query, CancellationToken ct = default) => repository.GetLookupAsync(query.Term, ct);
}

public sealed class InquiryValidationException(string message) : Exception(message);
public sealed class InquiryNotFoundException(string message) : Exception(message);
