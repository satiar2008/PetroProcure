using Microsoft.Extensions.Options;
using PetroProcure.Domain.Modules.Items;

namespace PetroProcure.Application.Mesc;

public sealed record CreateMescGeneralGroupCommand(string Code, string GeneralDescription);
public sealed record UpdateMescGeneralGroupCommand(Guid Id, string Code, string GeneralDescription);
public sealed record CreateMescItemCommand(string Code, string SpecificDescription, string UnitOfMeasure, string? GeneralDescription = null, Guid? UnitOfMeasureId = null);
public sealed record UpdateMescItemCommand(Guid Id, string Code, string SpecificDescription, string UnitOfMeasure, string? GeneralDescription = null, Guid? UnitOfMeasureId = null);
public sealed record ActivateMescItemCommand(Guid Id);
public sealed record DeactivateMescItemCommand(Guid Id);

public sealed class MescCommandHandler(
    IMescCatalogRepository repository,
    IOptions<MescCatalogOptions> options)
{
    public async Task<MescGeneralGroupDto> Handle(CreateMescGeneralGroupCommand command, CancellationToken cancellationToken = default)
    {
        var code = MescGeneralGroup.ValidateCode(command.Code);
        if (await repository.GeneralGroupCodeExistsAsync(code, null, cancellationToken))
            throw new MescCatalogConflictException($"General group '{code}' already exists.");

        var group = new MescGeneralGroup(Guid.NewGuid(), code, command.GeneralDescription);
        await repository.AddGeneralGroupAsync(group, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new(group.Id, group.Code, group.Description, group.IsActive);
    }

    public async Task<MescGeneralGroupDto> Handle(UpdateMescGeneralGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindGeneralGroupAsync(command.Id, cancellationToken)
            ?? throw new MescCatalogNotFoundException("General group was not found.");
        var code = MescGeneralGroup.ValidateCode(command.Code);
        if (await repository.GeneralGroupCodeExistsAsync(code, command.Id, cancellationToken))
            throw new MescCatalogConflictException($"General group '{code}' already exists.");

        group.Update(code, command.GeneralDescription);
        await repository.SaveChangesAsync(cancellationToken);
        return new(group.Id, group.Code, group.Description, group.IsActive);
    }

    public async Task<MescItemDto> Handle(CreateMescItemCommand command, CancellationToken cancellationToken = default)
    {
        var code = MescItem.ValidateCode(command.Code, options.Value.AllowNonNumericItemCodes);
        if (await repository.ItemCodeExistsAsync(code, null, cancellationToken))
            throw new MescCatalogConflictException($"MESC item '{code}' already exists.");

        var group = await ResolveGroup(code, command.GeneralDescription, cancellationToken);
        var unitOfMeasure = NormalizeAllowedUnit(command.UnitOfMeasure)
            ?? throw new MescCatalogValidationException($"Unit of measure '{command.UnitOfMeasure}' is not allowed.");
        var unitId = await ResolveUnit(unitOfMeasure, command.UnitOfMeasureId, cancellationToken);
        var item = new MescItem(Guid.NewGuid(), code, command.SpecificDescription, unitOfMeasure, unitId);
        item.LinkGeneralGroup(group);
        await repository.AddItemAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(item, group);
    }

    public async Task<MescItemDto> Handle(UpdateMescItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await repository.FindItemAsync(command.Id, cancellationToken)
            ?? throw new MescCatalogNotFoundException("MESC item was not found.");
        var code = MescItem.ValidateCode(command.Code, options.Value.AllowNonNumericItemCodes);
        if (await repository.ItemCodeExistsAsync(code, command.Id, cancellationToken))
            throw new MescCatalogConflictException($"MESC item '{code}' already exists.");

        var group = await ResolveGroup(code, command.GeneralDescription, cancellationToken);
        var unitOfMeasure = NormalizeAllowedUnit(command.UnitOfMeasure)
            ?? throw new MescCatalogValidationException($"Unit of measure '{command.UnitOfMeasure}' is not allowed.");
        var unitId = await ResolveUnit(unitOfMeasure, command.UnitOfMeasureId, cancellationToken);
        item.Update(code, command.SpecificDescription, unitOfMeasure, unitId);
        item.LinkGeneralGroup(group);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(item, group);
    }

    public Task Handle(ActivateMescItemCommand command, CancellationToken cancellationToken = default) =>
        SetActive(command.Id, true, cancellationToken);

    public Task Handle(DeactivateMescItemCommand command, CancellationToken cancellationToken = default) =>
        SetActive(command.Id, false, cancellationToken);

    private async Task SetActive(Guid id, bool active, CancellationToken cancellationToken)
    {
        var item = await repository.FindItemAsync(id, cancellationToken)
            ?? throw new MescCatalogNotFoundException("MESC item was not found.");
        if (active) item.Activate(); else item.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<MescGeneralGroup> ResolveGroup(string itemCode, string? generalDescription, CancellationToken cancellationToken)
    {
        var groupCode = itemCode[..6];
        var group = await repository.FindGeneralGroupByCodeAsync(groupCode, cancellationToken);
        if (group is not null) return group;

        if (!options.Value.AutoCreateGeneralGroup)
            throw new MescCatalogValidationException($"General group '{groupCode}' does not exist.");
        if (string.IsNullOrWhiteSpace(generalDescription))
            throw new MescCatalogValidationException("General description is required when automatically creating a group.");

        group = new MescGeneralGroup(Guid.NewGuid(), groupCode, generalDescription);
        await repository.AddGeneralGroupAsync(group, cancellationToken);
        return group;
    }

    private async Task<Guid> ResolveUnit(string unitOfMeasure, Guid? unitOfMeasureId, CancellationToken cancellationToken)
    {
        var resolved = await repository.ResolveUnitOfMeasureIdAsync(unitOfMeasure, cancellationToken);
        if (resolved == Guid.Empty) throw new MescCatalogValidationException($"Unit of measure '{unitOfMeasure}' does not exist.");
        if (unitOfMeasureId.HasValue && unitOfMeasureId.Value != Guid.Empty && unitOfMeasureId.Value != resolved)
            throw new MescCatalogValidationException($"Unit of measure '{unitOfMeasure}' does not match the selected unit id.");
        return resolved;
    }

    private static string? NormalizeAllowedUnit(string unitOfMeasure) =>
        string.IsNullOrWhiteSpace(unitOfMeasure) ? null : unitOfMeasure.Trim() switch
        {
            "EA" or "عدد" => "عدد",
            "PKG" or "بسته" => "بسته",
            "KG" or "کیلوگرم" => "کیلوگرم",
            "M" or "متر" => "متر",
            _ => null
        };

    private static MescItemDto ToDto(MescItem item, MescGeneralGroup group) =>
        new(item.Id, item.Code, item.GeneralGroupCode, group.Description, item.Description, item.UnitOfMeasure, item.UnitOfMeasureId, item.IsActive);
}

public sealed class MescCatalogValidationException(string message) : Exception(message);
public sealed class MescCatalogNotFoundException(string message) : Exception(message);
public sealed class MescCatalogConflictException(string message) : Exception(message);
