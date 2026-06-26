# Legal Rules Management

Admins manage tender law knowledge in three layers:

1. Legal document metadata and source file.
2. Legal articles and clauses derived from the source document.
3. Versioned executable procurement rules referencing articles or clauses.

Editing rule versions:

- Active rule versions are immutable.
- To change an active rule, clone it as draft.
- Draft versions can be edited.
- Draft versions are submitted for approval.
- Approval activates the new version and deprecates the old active version.

Admin UX:

- `/admin/legal-documents` uploads and lists legal documents.
- `/admin/legal-documents/{id}` manages articles and clauses.
- `/admin/procurement-rules` creates rules.
- `/admin/procurement-rules/{id}/edit` manages versions.
- `LegalClauseSelector` lets admins search/select clauses without raw GUID entry.

Every rule must keep a legal reference. Evaluations store the exact rule version used so historical purchase files remain stable.
