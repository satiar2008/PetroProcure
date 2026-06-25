# Contract Management Foundation

Phase 34 adds the foundation for managing purchase contracts in PetroProcure.

## Scope

Implemented scope:

- Purchase Contract aggregate with status lifecycle.
- Contract number format: `CNT-YYYY-000001`.
- Contract creation manually, from Purchase File, and from Tender/Tender Bid.
- Snapshot contract items with MESC code, general group code, descriptions, unit, quantity, and price fields.
- Contract clauses with required/editable flags.
- Contract approvals and status changes.
- Contract document metadata linked to the existing Purchase File document repository.
- DevExpress PDF report for official contract print/export.
- Persian RTL MudBlazor Web pages for contract list, creation, details, clauses, documents, approvals, and reports.

Out of scope for this phase:

- Purchase Order generation from contract.
- Warehouse receipt, invoice, payment, finance, and delivery tracking.
- Electronic signature integration.
- External contract approval engines.

## Key business rules

- A contract belongs to one Purchase File.
- A contract must have a supplier.
- Contract items keep snapshot descriptions and MESC grouping so later item catalog changes do not change historical contracts.
- A contract cannot be submitted without at least one item and at least one required clause.
- Approved, signed, active, completed, and archived contracts are read-only for normal edits.
- Signed contracts are not considered final business payment/order execution; later modules will connect them to Purchase Orders and Warehouse Receipts.

## API routes

Main routes:

- `GET /api/contracts`
- `GET /api/contracts/{id}`
- `GET /api/contracts/by-number/{contractNumber}`
- `POST /api/contracts`
- `POST /api/contracts/from-purchase-file/{purchaseFileId}`
- `POST /api/contracts/from-tender/{tenderId}`
- `POST /api/contracts/from-tender-bid/{tenderBidId}`
- `POST /api/contracts/{id}/submit`
- `POST /api/contracts/{id}/approve`
- `POST /api/contracts/{id}/reject`
- `POST /api/contracts/{id}/sign`
- `POST /api/contracts/{id}/cancel`
- `GET /api/contracts/{id}/reports/contract/pdf`
- `POST /api/contracts/{id}/reports/contract/save-to-file`

All routes use server-side authenticated user context. Client requests must not send user identity values.

## Permissions

Contract permissions:

- `Contract.View`
- `Contract.Create`
- `Contract.Edit`
- `Contract.Submit`
- `Contract.Approve`
- `Contract.Reject`
- `Contract.Sign`
- `Contract.Cancel`
- `Contract.ManageClauses`
- `Contract.ManageTemplates`
- `Contract.ManageDocuments`
- `Contract.ReportView`
- `Contract.ReportExport`

## Web routes

- `/purchase/contracts`
- `/purchase/contracts/create`
- `/purchase/contracts/{id}`
- `/purchase/contracts/from-purchase-file/{purchaseFileId}`
- `/purchase/contracts/from-tender/{tenderId}`
- `/purchase/contracts/templates`

