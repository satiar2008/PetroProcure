using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFileSequence : Entity<Guid>
{
    public PurchaseFileSequence(Guid id, int year, int lastSequence) : base(id)
    {
        Year = year;
        LastSequence = lastSequence;
    }

    public int Year { get; private set; }
    public int LastSequence { get; private set; }

    public int Next()
    {
        if (LastSequence >= 999999) throw new InvalidOperationException("Purchase file sequence is exhausted.");
        return ++LastSequence;
    }
}
