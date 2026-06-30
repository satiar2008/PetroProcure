using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Security;
using PetroProcure.Application.Legal;

namespace PetroProcure.Application.PurchaseFiles;

public sealed record CreatePurchaseFileCommand(
    int Year, string Title, string? Description, PurchaseFilePriority Priority,
    Guid PurchaseDepartmentId, Guid CurrentDepartmentId, Guid? ResponsibleUserId);
public sealed record CreatePurchaseFileFromIndentCommand(
    Guid IndentId, int Year, Guid PurchaseDepartmentId, Guid? ResponsibleUserId,
    PurchaseFilePriority Priority = PurchaseFilePriority.Normal);
public sealed record AddPurchaseFileItemCommand(
    Guid PurchaseFileId, Guid MescItemId, Guid UnitOfMeasureId, decimal RequestedQuantity,
    decimal ApprovedQuantity, string? TechnicalDescription);
public sealed record RemovePurchaseFileItemCommand(Guid PurchaseFileId, Guid ItemId);
public sealed record AssignPurchaseFileToDepartmentCommand(
    Guid Id, Guid DepartmentId, Guid? ResponsibleUserId, string? Reason);
public sealed record ChangePurchaseFileStatusCommand(
    Guid Id, PurchaseFileStatus Status, string? Reason, Guid? DepartmentId);
public sealed record AddPurchaseFileNoteCommand(
    Guid Id, Guid DepartmentId, string NoteText, bool IsInternal);
public sealed record CompletePurchaseFileCommand(Guid Id, string? Reason);
public sealed record ArchivePurchaseFileCommand(Guid Id, string? Reason);

public sealed class PurchaseFileCommandHandler(
    IPurchaseFileRepository repository, IPurchaseFileNumberService numberService,
    ICurrentUserService currentUser,
    IFileStorageService? fileStorageService = null,
    IProcurementRuleGateService? gateService = null)
{
    public async Task<PurchaseFileDto> Handle(CreatePurchaseFileCommand command, CancellationToken ct = default)
    {
        RequireDepartment(command.CurrentDepartmentId);
        var number = await numberService.GenerateNextFileNumber(command.Year, ct);
        await EnsureUnique(number, ct);
        var file = new PurchaseFile(
            Guid.NewGuid(), number, command.Title, command.Description, command.Priority, null,
            command.PurchaseDepartmentId, command.CurrentDepartmentId, command.ResponsibleUserId, currentUser.UserId);
        ApplyInitialWorkflowStatus(file, command.PurchaseDepartmentId, command.CurrentDepartmentId);
        await repository.AddAsync(file, ct);
        await repository.SaveChangesAsync(ct);
        if (fileStorageService is not null) await fileStorageService.EnsurePurchaseFileFoldersAsync(file, ct);
        return ToDto(file);
    }

    public async Task<PurchaseFileDto> Handle(CreatePurchaseFileFromIndentCommand command, CancellationToken ct = default)
    {
        if (await repository.FindBySourceIndentAsync(command.IndentId, true, ct) is { } existingFile)
            return ToDto(existingFile);
        var indent = await repository.FindApprovedIndentAsync(command.IndentId, ct)
            ?? throw new PurchaseFileValidationException("Only an approved or purchase-sent indent can create a purchase file.");
        var number = await numberService.GenerateNextFileNumber(command.Year, ct);
        await EnsureUnique(number, ct);
        var file = new PurchaseFile(
            Guid.NewGuid(), number, indent.Title, indent.Description, command.Priority, indent.Id,
            command.PurchaseDepartmentId, command.PurchaseDepartmentId, command.ResponsibleUserId, currentUser.UserId);
        ApplyInitialWorkflowStatus(file, command.PurchaseDepartmentId, command.PurchaseDepartmentId);
        foreach (var source in indent.Items)
        {
            file.AddItem(new PurchaseFileItem(
                Guid.NewGuid(), file.Id, source.MescItemId, source.MescCode, source.MescGeneralGroupCode,
                source.GeneralDescription, source.SpecificDescription, source.UnitOfMeasureId,
                source.RequestedQuantity, source.RequestedQuantity, source.TechnicalDescription, source.Id));
        }
        await repository.AddAsync(file, ct);
        await repository.SaveChangesAsync(ct);
        if (fileStorageService is not null) await fileStorageService.EnsurePurchaseFileFoldersAsync(file, ct);
        return ToDto(file);
    }

    public async Task<PurchaseFileItemDto> Handle(AddPurchaseFileItemCommand command, CancellationToken ct = default)
    {
        var file = await Required(command.PurchaseFileId, true, ct);
        var snapshot = await repository.GetMescSnapshotAsync(command.MescItemId, ct)
            ?? throw new PurchaseFileValidationException("MESC item was not found.");
        if (!snapshot.IsActive) throw new PurchaseFileValidationException("Inactive MESC items cannot be added.");
        if (!await repository.UnitOfMeasureExistsAsync(command.UnitOfMeasureId, ct))
            throw new PurchaseFileValidationException("Unit of measure was not found.");
        var item = new PurchaseFileItem(
            Guid.NewGuid(), file.Id, snapshot.Id, snapshot.Code, snapshot.GeneralGroupCode,
            snapshot.GeneralDescription, snapshot.SpecificDescription, command.UnitOfMeasureId,
            command.RequestedQuantity, command.ApprovedQuantity, command.TechnicalDescription, null);
        Execute(() => file.AddItem(item, currentUser.IsSystemAdmin));
        await repository.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task Handle(RemovePurchaseFileItemCommand command, CancellationToken ct = default)
    {
        var file = await Required(command.PurchaseFileId, true, ct);
        Execute(() => file.RemoveItem(command.ItemId, currentUser.IsSystemAdmin));
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(AssignPurchaseFileToDepartmentCommand command, CancellationToken ct = default)
    {
        var file = await Required(command.Id, true, ct);
        Execute(() => file.AssignDepartment(command.DepartmentId, command.ResponsibleUserId, currentUser.UserId, command.Reason, currentUser.IsSystemAdmin));
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(ChangePurchaseFileStatusCommand command, CancellationToken ct = default)
    {
        var file = await Required(command.Id, true, ct);
        Execute(() => file.ChangeStatus(command.Status, currentUser.UserId, command.Reason, command.DepartmentId, currentUser.IsSystemAdmin));
        await repository.SaveChangesAsync(ct);
    }

    public async Task<PurchaseFileNoteDto> Handle(AddPurchaseFileNoteCommand command, CancellationToken ct = default)
    {
        var file = await Required(command.Id, true, ct);
        RequireDepartment(command.DepartmentId);
        var note = new PurchaseFileNote(Guid.NewGuid(), file.Id, command.DepartmentId, currentUser.UserId, command.NoteText, command.IsInternal);
        Execute(() => file.AddNote(note, currentUser.IsSystemAdmin));
        await repository.SaveChangesAsync(ct);
        return ToDto(note);
    }

    public Task Handle(CompletePurchaseFileCommand command, CancellationToken ct = default) =>
        Mutate(command.Id, file => file.Complete(currentUser.UserId, command.Reason),
            ProcurementRuleGateTransitions.PurchaseFileComplete, ct);
    public Task Handle(ArchivePurchaseFileCommand command, CancellationToken ct = default) =>
        Mutate(command.Id, file => file.Archive(currentUser.UserId, command.Reason),
            ProcurementRuleGateTransitions.PurchaseFileArchive, ct);

    private async Task Mutate(Guid id, Action<PurchaseFile> action, string? gatedTransitionName, CancellationToken ct)
    {
        var file = await Required(id, true, ct);
        if (gatedTransitionName is not null)
            await EnsureGateAllowsAsync(file.Id, gatedTransitionName, ct);
        Execute(() => action(file));
        await repository.SaveChangesAsync(ct);
    }

    private async Task EnsureUnique(string number, CancellationToken ct)
    {
        if (await repository.FileNumberExistsAsync(number, ct))
            throw new PurchaseFileConflictException($"Purchase file '{number}' already exists.");
    }

    private void ApplyInitialWorkflowStatus(PurchaseFile file, Guid purchaseDepartmentId, Guid currentDepartmentId)
    {
        var status = currentDepartmentId == purchaseDepartmentId
            ? PurchaseFileStatus.InPurchaseDepartment
            : PurchaseFileStatus.WaitingForPurchaseDepartment;
        Execute(() => file.ChangeStatus(status, currentUser.UserId, "Initial purchase file routing.", currentDepartmentId, currentUser.IsSystemAdmin));
    }

    private async Task<PurchaseFile> Required(Guid id, bool details, CancellationToken ct) =>
        await repository.FindAsync(id, details, ct) ?? throw new PurchaseFileNotFoundException("Purchase file was not found.");

    private void RequireDepartment(Guid departmentId)
    {
        if (!currentUser.IsSystemAdmin && !currentUser.DepartmentIds.Contains(departmentId))
            throw new CurrentUserForbiddenException("User does not have access to the department.");
    }

    private static void Execute(Action action)
    {
        try { action(); }
        catch (InvalidOperationException ex) { throw new PurchaseFileValidationException(ex.Message); }
    }

    private async Task EnsureGateAllowsAsync(Guid entityId, string transitionName, CancellationToken ct)
    {
        if (gateService is null) return;
        var result = await gateService.CheckTransitionAsync(entityId, transitionName,
            new ProcurementRuleGateUserContext(currentUser.UserId, currentUser.IsSystemAdmin, currentUser.Permissions), ct);
        if (result.IsBlocked)
            throw new LegalRuleValidationException("Sensitive transition is blocked by unresolved legal rule findings.");
    }

    internal static PurchaseFileDto ToDto(PurchaseFile file) => new(
        file.Id, file.FileNumber, file.Title, file.Description, file.Status, file.Priority,
        file.SourceIndentId, file.PurchaseDepartmentId, file.CurrentDepartmentId, file.ResponsibleUserId,
        file.CreatedByUserId, file.CreatedAt, file.CompletedAt, file.ArchivedAt,
        file.Items.Select(ToDto).ToArray(), file.StatusHistory.Select(ToDto).ToArray(), file.Notes.Select(ToDto).ToArray());
    internal static PurchaseFileItemDto ToDto(PurchaseFileItem item) => new(
        item.Id, item.MescItemId, item.MescCode, item.MescGeneralGroupCode, item.GeneralDescription,
        item.SpecificDescription, item.UnitOfMeasureId, item.RequestedQuantity, item.ApprovedQuantity,
        item.TechnicalDescription, item.SourceIndentItemId);
    internal static PurchaseFileStatusHistoryDto ToDto(PurchaseFileStatusHistory history) => new(
        history.Id, history.FromStatus, history.ToStatus, history.ChangedByUserId,
        history.ChangedAt, history.Reason, history.DepartmentId);
    internal static PurchaseFileNoteDto ToDto(PurchaseFileNote note) => new(
        note.Id, note.DepartmentId, note.UserId, note.NoteText, note.CreatedAt, note.IsInternal);
}

public sealed class PurchaseFileValidationException(string message) : Exception(message);
public sealed class PurchaseFileNotFoundException(string message) : Exception(message);
public sealed class PurchaseFileConflictException(string message) : Exception(message);
