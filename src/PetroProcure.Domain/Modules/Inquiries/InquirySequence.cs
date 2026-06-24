using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class InquirySequence : Entity<Guid>
{
    private InquirySequence() : base(Guid.Empty) { }
    public InquirySequence(Guid id, int year, int lastSequence) : base(id) { Year = year; LastSequence = lastSequence; }
    public int Year { get; private set; }
    public int LastSequence { get; private set; }
    public int Next() => ++LastSequence;
}
