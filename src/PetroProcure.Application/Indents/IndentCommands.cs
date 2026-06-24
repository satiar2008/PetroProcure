using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Application.Security;
using PetroProcure.Application.Workflow;

namespace PetroProcure.Application.Indents;

public sealed record CreateIndentCommand(
    int YearPart, int TypeDigit, string Title, Guid RequestingDepartmentId,
    Guid? ApplicantDepartmentId, string? Description);
public sealed record AddIndentItemCommand(
    Guid IndentId, Guid MescItemId, Guid UnitOfMeasureId, decimal RequestedQuantity,
    string? TechnicalDescription, DateOnly? RequiredDate);
public sealed record RemoveIndentItemCommand(Guid IndentId, Guid ItemId);
public sealed record SubmitIndentCommand(Guid Id);
public sealed record ApproveIndentCommand(Guid Id);
public sealed record RejectIndentCommand(Guid Id);
public sealed record SendIndentToPurchaseDepartmentCommand(Guid Id);

public sealed class IndentCommandHandler(
    IIndentRepository repository,
    IIndentNumberService numberService,
    ICurrentUserService currentUser,
    IWorkflowRepository? workflowRepository = null)
{
    public async Task<IndentDto> Handle(CreateIndentCommand command, CancellationToken cancellationToken = default)
    {
        RequireDepartment(command.RequestingDepartmentId);
        var number = await numberService.GenerateNextIndentNumber(command.YearPart, command.TypeDigit, cancellationToken);
        var parts = numberService.ParseIndentNumber(number);
        if (await repository.IndentNumberExistsAsync(number, cancellationToken))
            throw new IndentConflictException($"Indent number '{number}' already exists.");

        var indent = new Indent(
            Guid.NewGuid(), number, parts.YearPart, parts.TypeDigit, parts.Sequence, command.Title,
            command.RequestingDepartmentId, command.ApplicantDepartmentId, currentUser.UserId, command.Description);
        await repository.AddAsync(indent, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(indent);
    }

    public async Task<IndentItemDto> Handle(AddIndentItemCommand command, CancellationToken cancellationToken = default)
    {
        var indent = await RequiredIndent(command.IndentId, true, cancellationToken);
        var snapshot = await repository.GetMescItemSnapshotAsync(command.MescItemId, cancellationToken)
            ?? throw new IndentValidationException("MESC item was not found.");
        if (!snapshot.IsActive) throw new IndentValidationException("Inactive MESC items cannot be added.");
        if (!await repository.UnitOfMeasureExistsAsync(command.UnitOfMeasureId, cancellationToken))
            throw new IndentValidationException("Unit of measure was not found.");

        var item = new IndentItem(
            Guid.NewGuid(), indent.Id, snapshot.Id, snapshot.Code, snapshot.GeneralGroupCode,
            snapshot.GeneralDescription, snapshot.SpecificDescription,
            command.UnitOfMeasureId == Guid.Empty ? snapshot.UnitOfMeasureId : command.UnitOfMeasureId,
            command.RequestedQuantity, command.TechnicalDescription, command.RequiredDate);
        indent.AddItem(item);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task Handle(RemoveIndentItemCommand command, CancellationToken cancellationToken = default)
    {
        var indent = await RequiredIndent(command.IndentId, true, cancellationToken);
        indent.RemoveItem(command.ItemId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public Task Handle(SubmitIndentCommand command, CancellationToken cancellationToken = default) =>
        ChangeStatus(command.Id, indent => indent.Submit(), cancellationToken);
    public Task Handle(ApproveIndentCommand command, CancellationToken cancellationToken = default) =>
        ChangeStatus(command.Id, indent => indent.Approve(), cancellationToken);
    public Task Handle(RejectIndentCommand command, CancellationToken cancellationToken = default) =>
        ChangeStatus(command.Id, indent => indent.Reject(), cancellationToken);
    public async Task Handle(SendIndentToPurchaseDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var indent = await RequiredIndent(command.Id, true, cancellationToken);
        var fromDepartmentId = indent.RequestingDepartmentId;
        try { indent.SendToPurchaseDepartment(); }
        catch (InvalidOperationException exception) { throw new IndentValidationException(exception.Message); }

        if (workflowRepository is not null)
        {
            var purchaseDepartmentId = await repository.GetDepartmentIdByTypeAsync(
                DepartmentType.PurchaseDepartment, cancellationToken)
                ?? throw new IndentValidationException("Purchase department was not found.");
            var workflow = new WorkflowInstance(Guid.NewGuid(), "Indent", indent.Id,
                fromDepartmentId, currentUser.UserId);
            workflow.Send(purchaseDepartmentId, "SendIndentToPurchaseDepartment",
                $"تقاضای شماره {indent.IndentNumber} به واحد خرید ارسال شد.", currentUser.UserId);
            await workflowRepository.AddWorkflowAsync(workflow, cancellationToken);
            await workflowRepository.AddTaskAsync(new InboxTask(
                Guid.NewGuid(), workflow.Id, null, indent.Id, purchaseDepartmentId, null,
                $"بررسی تقاضای خرید {indent.IndentNumber}",
                $"تقاضای «{indent.Title}» از واحد سفارشات/درخواست‌کننده به واحد خرید ارسال شده است. پس از بررسی، امکان تشکیل پرونده خرید وجود دارد.",
                null), cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ChangeStatus(Guid id, Action<Indent> change, CancellationToken cancellationToken)
    {
        var indent = await RequiredIndent(id, true, cancellationToken);
        try { change(indent); }
        catch (InvalidOperationException exception) { throw new IndentValidationException(exception.Message); }
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Indent> RequiredIndent(Guid id, bool includeItems, CancellationToken cancellationToken) =>
        await repository.FindAsync(id, includeItems, cancellationToken)
        ?? throw new IndentNotFoundException("Indent was not found.");

    private void RequireDepartment(Guid departmentId)
    {
        if (!currentUser.IsSystemAdmin && !currentUser.DepartmentIds.Contains(departmentId))
            throw new CurrentUserForbiddenException("User does not have access to the requesting department.");
    }

    internal static IndentDto ToDto(Indent indent) => new(
        indent.Id, indent.IndentNumber, indent.YearPart, indent.TypeDigit, indent.Sequence, indent.IndentType,
        indent.Title, indent.RequestingDepartmentId, indent.ApplicantDepartmentId, indent.CreatedByUserId,
        indent.CreatedAt, indent.Status, indent.Description, indent.SourceType, indent.SourceDescription,
        indent.SourceReferenceId, indent.SourceDisplayText, indent.Items.Select(ToDto).ToArray());

    internal static IndentItemDto ToDto(IndentItem item) => new(
        item.Id, item.MescItemId, item.MescCode, item.MescGeneralGroupCode, item.GeneralDescription,
        item.SpecificDescription, item.UnitOfMeasureId, item.RequestedQuantity,
        item.TechnicalDescription, item.RequiredDate);
}

public sealed class IndentValidationException(string message) : Exception(message);
public sealed class IndentNotFoundException(string message) : Exception(message);
public sealed class IndentConflictException(string message) : Exception(message);
