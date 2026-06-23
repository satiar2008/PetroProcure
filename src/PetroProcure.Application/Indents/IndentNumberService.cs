using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;

namespace PetroProcure.Application.Indents;

public interface IIndentNumberService
{
    Task<string> GenerateNextIndentNumber(int yearPart, int typeDigit, CancellationToken cancellationToken = default);
    bool ValidateIndentNumber(string indentNumber);
    IndentNumberParts ParseIndentNumber(string indentNumber);
    IndentType ResolveIndentType(int typeDigit);
}

public sealed class IndentNumberService(IIndentRepository repository) : IIndentNumberService
{
    public Task<string> GenerateNextIndentNumber(int yearPart, int typeDigit, CancellationToken cancellationToken = default)
    {
        ValidateParts(yearPart, typeDigit);
        return repository.GenerateNextIndentNumberAsync(yearPart, typeDigit, cancellationToken);
    }

    public bool ValidateIndentNumber(string indentNumber)
    {
        try { ParseIndentNumber(indentNumber); return true; }
        catch (ArgumentOutOfRangeException) { return false; }
        catch (ArgumentException) { return false; }
    }

    public IndentNumberParts ParseIndentNumber(string indentNumber)
    {
        var normalized = indentNumber?.Replace(" ", string.Empty);
        if (normalized is null || normalized.Length != 7 || !normalized.All(char.IsDigit))
            throw new ArgumentException("Indent number must contain exactly 7 digits.", nameof(indentNumber));

        var yearPart = int.Parse(normalized[..2]);
        var typeDigit = normalized[2] - '0';
        var sequence = int.Parse(normalized[3..]);
        if (sequence < 1) throw new ArgumentException("Indent sequence must be between 0001 and 9999.", nameof(indentNumber));
        var type = ResolveIndentType(typeDigit);
        return new(normalized, yearPart, typeDigit, sequence, type);
    }

    public IndentType ResolveIndentType(int typeDigit) => Indent.ResolveIndentType(typeDigit);

    private static void ValidateParts(int yearPart, int typeDigit)
    {
        if (yearPart is < 0 or > 99) throw new ArgumentOutOfRangeException(nameof(yearPart));
        Indent.ResolveIndentType(typeDigit);
    }
}
