# Rule Engine

PetroProcure evaluates procurement rules deterministically first. Automatic rules produce final pass/fail/warning results. SemiAutomatic and ManualReview rules may request AI assistance, but AI findings remain advisory and require human review before they can become blocking decisions.

Core concepts:
- `PurchaseFile` is the central aggregate.
- `ProcurementRuleVersion` defines the active rule logic, severity, evaluation mode, and legal reference.
- `ProcurementRuleEvaluation` stores one evaluation run for an entity.
- `ProcurementRuleFinding` stores rule findings, including AI advisory flags and citation references.

Safety rules:
- AI never creates final blocking decisions by itself.
- Blocking workflow gates use deterministic failed findings or human-approved override audit entries.
- Evaluation failures from AiCore must not break deterministic evaluation.

