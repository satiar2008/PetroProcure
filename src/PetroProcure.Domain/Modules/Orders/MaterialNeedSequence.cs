using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Orders;

public sealed class MaterialNeedSequence : AuditableEntity<Guid>
{
    private MaterialNeedSequence() : base(Guid.Empty) { }

    public MaterialNeedSequence(Guid id, int year, int lastSequence) : base(id)
    {
        Year = year;
        LastSequence = lastSequence;
    }

    public int Year { get; private set; }
    public int LastSequence { get; private set; }

    public int Next()
    {
        LastSequence++;
        return LastSequence;
    }
}
