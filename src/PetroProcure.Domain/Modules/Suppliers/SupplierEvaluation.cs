using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class SupplierEvaluation : Entity<Guid>
{
    private SupplierEvaluation()
        : base(Guid.Empty)
    {
    }

    public SupplierEvaluation(Guid id, Guid supplierId, DateTime evaluationDate, decimal? score, SupplierEvaluationResult result, Guid evaluatedByUserId, string? description)
        : base(id)
    {
        SupplierId = supplierId;
        EvaluationDate = evaluationDate;
        Score = score;
        Result = result;
        EvaluatedByUserId = evaluatedByUserId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public DateTime EvaluationDate { get; private set; }
    public decimal? Score { get; private set; }
    public SupplierEvaluationResult Result { get; private set; }
    public Guid EvaluatedByUserId { get; private set; }
    public string? Description { get; private set; }
}
