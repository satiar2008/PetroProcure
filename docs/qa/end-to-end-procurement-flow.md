# End-to-End Procurement Flow (Phase 39 QA)

This document maps the complete PetroProcure business process — from the first material need to warehouse receipt and inventory update — to the actual API endpoints, expected status transitions, and expected side effects (documents, inventory, legal/AI). It is the reference for both automated end-to-end integration tests and manual QA.

> Phase 39.1 validation note: `dotnet restore`, `dotnet build --no-restore`, and `dotnet test --no-build` were run successfully on 2026-06-26. Final totals after the lifecycle endpoint regression were 308 passing tests: 227 Unit, 69 Integration, 12 Architecture.

## Test data strategy
- Prefer creating required data **inside** each test (department-scoped clients, fresh suppliers/indents/files) rather than relying on fragile seed rows.
- Stable seed anchors that are safe to reuse: `IdentitySeedData.DefaultAdminUserId` / `DefaultAdminUserName`, `SeedDataIds.OrdersAndInventoryControlId`, `SeedDataIds.PipeItemId`, `SeedDataIds.GateValveItemId`, `SeedDataIds.EachUnitId`. `SamplePurchaseFileId` / `SampleIndentId` exist but prefer creating your own for lifecycle isolation.
- Auth in tests: `ApiAuthorizationFactory.CreateAuthenticatedClient(permission, departmentId, userId)` (sends `X-Test-User` / `X-Test-Permission` / `X-Test-Department`). One permission per client; create a new client per permission scope.

## The 52-step flow

| # | Step | Endpoint (method) | Permission | Expected status / result | Side effects |
|---|------|-------------------|------------|--------------------------|--------------|
| 1 | Create MaterialNeed | `POST /api/orders/material-needs` | `Orders.CreateMaterialNeed` (+dept) | 201, `MaterialNeedDto`, status Draft | — |
| 2 | Submit MaterialNeed | `POST /api/orders/material-needs/{id}/submit` | `Orders.CreateMaterialNeed` | 204, status Submitted | — |
| 3 | Approve MaterialNeed | `POST /api/orders/material-needs/{id}/approve` | `Orders.ApproveMaterialNeed` | 204, status Approved | — |
| 4 | Convert to Indent | `POST /api/orders/material-needs/{id}/convert-to-indent` | `Orders.ConvertNeedToIndent` (+dept) | 200, `IndentReference` | Indent created |
| 5 | Indent snapshot items | `GET /api/indents/{id}` | `Indent.View` | SourceType=MaterialNeed, SourceReferenceId=need.Id, MESC snapshot items present | — |
| 6 | Submit Indent | `POST /api/indents/{id}/submit` | `Indent.Create` | status Submitted | — |
| 7 | Approve Indent | `POST /api/indents/{id}/approve` | `Indent.Approve` | status Approved | — |
| 8 | Send to Purchase Dept | `POST /api/indents/{id}/send-to-purchase` | `Indent.SendToPurchase` | status SentToPurchase | WorkflowInstance + WorkflowStep + InboxTask(Purchase) created |
| 9 | Verify workflow/inbox | inbox endpoints | AiAgentUse/Inbox perms | InboxTask exists for Purchase dept | — |
| 10–11 | Create PurchaseFile from Indent | `POST /api/purchase-files/from-indent/{indentId}` | `PurchaseFile.Create` | 201, `PurchaseFileDto` | PurchaseFile linked to source indent |
| 12 | PurchaseFile grouped items | `GET /api/purchase-files/{id}/items/grouped` | `PurchaseFile.View` | grouped by MESC general group; counts match indent | — |
| 13 | Create Inquiry from PF | `POST /api/inquiries/from-purchase-file/{pfId}` | `Inquiry.Create` | 201, `InquiryDetailDto`, items+suppliers populated | — |
| 14 | Add supplier | (included in create or `POST /api/inquiries/{id}/suppliers`) | `Inquiry.ManageSuppliers` | supplier on inquiry | — |
| 15 | Send Inquiry | `POST /api/inquiries/{id}/send` | `Inquiry.Send` | 204, status Sent | — |
| 16 | Register quote | `POST /api/inquiries/{id}/quotes` | `Inquiry.ReceiveQuote` | 201 | — |
| 17 | Comparison | `GET /api/inquiries/{id}/comparison` | `Inquiry.CompareQuotes` | `InquiryComparisonDto` with suppliers | — |
| 18 | Create Tender | `POST /api/tenders` | `Tender.Create` | 201, `TenderDetailDto`, linked to PF | — |
| 19 | Add participant | `POST /api/tenders/{id}/participants` | `Tender.ManageParticipants` | participant added | — |
| 20 | Publish Tender | `POST /api/tenders/{id}/publish` | `Tender.Publish` | status Published | — |
| 21 | Register bid | `POST /api/tenders/{id}/bids` | `Tender.ReceiveBid` | bid created | — |
| 22 | Register evaluation | `POST /api/tenders/{id}/evaluations` | `Tender.Evaluate` | evaluation stored | — |
| 23 | Select winner | `POST /api/tenders/{id}/select-winner` | `Tender.SelectWinner` | bid status Selected | — |
| 24 | Commission from Tender | `POST /api/commission/sessions/from-tender/{tenderId}` | `Commission.Create` | 201, `TenderCommissionSessionDetailDto` | — |
| 25 | Add member | `POST /api/commission/sessions/{id}/members` | `Commission.ManageMembers` | member added | — |
| 26 | Add agenda item | (in create or `/agenda`) | CommissionManage | agenda item present | — |
| 27 | Add minute | `POST /api/commission/sessions/{id}/minutes` | CommissionManage | minute stored | — |
| 28 | Add decision | `POST /api/commission/sessions/{id}/decisions` | CommissionManage | decision stored | — |
| 29 | Complete + approve | `POST .../complete`, `POST .../approve` | CommissionApprove | status Approved | — |
| 30 | Commission/Tender reports → PF docs | `POST .../reports/minutes/save-to-file` etc. | *ReportExport | FileDocument linked to PF | report saved |
| 31 | Create Contract | `POST /api/contracts/from-purchase-file/{pfId}` or `POST /api/contracts/from-tender/{tenderId}` | `Contract.Create` | 201, `ContractDetailDto` | — |
| 32 | Apply template | `POST /api/contracts/{id}/apply-template/{templateId}` | `Contract.ManageTemplates` | template content applied | — |
| 33 | Submit Contract | `POST /api/contracts/{id}/submit` | `Contract.Submit` | status Submitted | — |
| 34 | Approve Contract | `POST /api/contracts/{id}/approve` | `Contract.Approve` | status Approved | — |
| 35 | Sign Contract | `POST /api/contracts/{id}/sign` | `Contract.Sign` | status Signed | — |
| 36 | Contract PDF → PF docs | `GET /api/contracts/{id}/reports/contract/pdf`, `POST /api/contracts/{id}/reports/contract/save-to-file` | `Contract.ReportExport` | PDF bytes non-empty; FileDocument linked to PF | — |
| 37 | Create PO from Contract | `POST /api/purchase-orders/from-contract/{contractId}` | `PurchaseOrder.Create` | 201, `PurchaseOrderDetailDto` | — |
| 38 | Submit PO | `POST /api/purchase-orders/{id}/submit` | `PurchaseOrder.Submit` | status Submitted | — |
| 39 | Approve PO | `POST /api/purchase-orders/{id}/approve` | `PurchaseOrder.Approve` | status Approved | — |
| 40 | Issue PO | `POST /api/purchase-orders/{id}/issue` | `PurchaseOrder.Issue` | status Issued | — |
| 41 | PO PDF → PF docs | `GET /api/purchase-orders/{id}/reports/purchase-order/pdf`, `POST /api/purchase-orders/{id}/reports/purchase-order/save-to-file` | `PurchaseOrder.ReportExport` | PDF non-empty; FileDocument linked to PF | — |
| 42 | Create receipt from issued PO | `POST /api/warehouse-receipts/from-purchase-order/{poId}` | `WarehouseReceipt.Create` | 201, status Draft | — |
| 43 | Submit receipt | `POST /api/warehouse-receipts/{id}/submit` | `WarehouseReceipt.Submit` | status Submitted | **no** stock change yet |
| 44 | Approve receipt | `POST /api/warehouse-receipts/{id}/approve` | `WarehouseReceipt.Approve` | status Approved | stock + transaction applied |
| 45 | PO item quantities | `GET /api/purchase-orders/{id}` | `PurchaseOrder.View` | ReceivedQuantity↑, RemainingQuantity↓ | — |
| 46 | PO status | same | — | PartiallyReceived or FullyReceived | — |
| 47 | StockBalance | `GET /api/inventory/stock-balances` | `Inventory.ViewStockBalance` | AvailableQuantity increased | — |
| 48 | InventoryTransaction | `GET /api/inventory/transactions` | `Inventory.ViewTransactions` | Receipt transaction created | — |
| 49 | Receipt PDF → PF docs | `GET /api/warehouse-receipts/{id}/reports/receipt/pdf`, `POST /api/warehouse-receipts/{id}/reports/receipt/save-to-file` | `WarehouseReceipt.ReportExport` | PDF non-empty; FileDocument linked to PF | — |
| 50 | Legal evaluation | `POST /api/procurement-rules/evaluate/purchase-file/{purchaseFileId}` | `ProcurementRule.Evaluate` | `ProcurementRuleEvaluation` stored with findings | — |
| 51 | AI advisory | `POST /api/ai/purchase-files/{purchaseFileId}/analyze` or `POST /api/ai/purchase-files/{purchaseFileId}/jobs/analyze` | `Ai.AnalyzePurchaseFile` | AI result/job stored (mock provider) | — |
| 52 | Lifecycle summary | `GET /api/purchase-files/{id}/lifecycle` | `PurchaseFile.View` | aggregated chain returned | — |

> Main procurement, warehouse, inventory, legal, report, and AI routes above were checked against `*Endpoints.cs` during Phase 39.1. Re-check before expanding tests if endpoint files change.

## Expected document trail on the PurchaseFile
After a full run, the PurchaseFile `documents` should contain (each with relative path + hash, soft-deletable):
Indent (source), Commission minutes/decision report, Contract PDF, Purchase Order PDF, Warehouse Receipt PDF, plus any technical/commercial docs uploaded along the way.

## Expected inventory invariants
- Stock and `InventoryTransaction` change **only** on receipt **approval** (not Draft/Submitted/Cancelled).
- `ReceivedQuantity` never exceeds ordered; `RemainingQuantity = Ordered − Σ received`.
- Multiple partial receipts accumulate; status flips to FullyReceived only when remaining reaches 0.

## Expected legal/AI output
- Legal evaluation persists `ProcurementRuleFinding` rows referencing the **RuleVersion** used (immutable history).
- AI analysis persists an `AiAnalysisEvaluation`/`AiEvaluationResult` with an advisory disclaimer; AI never mutates business state.
