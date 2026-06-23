using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Indents;

public sealed class IndentSequence : Entity<Guid>
{
    public IndentSequence(Guid id, int yearPart, int typeDigit, int lastSequence) : base(id)
    {
        YearPart = yearPart;
        TypeDigit = typeDigit;
        LastSequence = lastSequence;
    }

    public int YearPart { get; private set; }
    public int TypeDigit { get; private set; }
    public int LastSequence { get; private set; }

    public int Next()
    {
        if (LastSequence >= 9999) throw new InvalidOperationException("Indent sequence is exhausted.");
        return ++LastSequence;
    }
}
