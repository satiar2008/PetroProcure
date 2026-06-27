# Legal Compliance Workflow

Legal compliance is visible from the purchase file detail page in Persian RTL UI.

User-facing panels:
- `بررسی حقوقی و قوانین`
- `جستجوی هوشمند اسناد`
- `پرسش از پرونده`

Workflow gates protect sensitive transitions such as tender publish, winner approval, purchase order issue, purchase file completion, and archive.

Override policy:
- Only users with `LegalRule.OverrideBlockingFinding` can override blocking findings.
- Override requires a reason.
- Each override is written to `LegalRuleAuditLog`.

Reports:
- `LegalComplianceReport` summarizes purchase file status, evaluation counts, blocking failed rules, warnings, human-review findings, legal references, and override audit notes.

