# Regression Test Matrix (Phase 39)

Maps each module/concern to its test coverage and to the six end-to-end test files this phase introduces. Use it to track what exists vs. what still needs to be written, and as the checklist for the final green run.

## Validation commands (Phase 39.1 verified)
```bash
dotnet restore PetroProcure.sln
dotnet build PetroProcure.sln --no-restore   # expect 0 warnings / 0 errors
dotnet test  PetroProcure.sln --no-build      # expect all passing
```
Phase 39.1 result on 2026-06-26:
- Restore succeeded.
- Build succeeded with 0 warnings / 0 errors.
- Full-suite post-change total: 308 passing tests (227 Unit / 69 Integration / 12 Architecture).
- Added focused lifecycle endpoint regression: 1 passing integration test.

## New Phase 39 test files (target)
| File | Scope |
|---|---|
| `ProcurementEndToEndFlowTests.cs` | Full 52-step happy path, lifecycle correctness, snapshot integrity |
| `ProcurementLifecycleDocumentTests.cs` | Document upload/list/download/soft-delete per module + report save-to-file |
| `ProcurementWorkflowInboxRegressionTests.cs` | Send-indent → WorkflowInstance/Step/InboxTask; inbox reliability |
| `ProcurementAuthorizationRegressionTests.cs` | 403 matrix for Tender/Commission/Contract/PO/Warehouse/Legal/AI; no client-trusted identity |
| `ProcurementInventoryRegressionTests.cs` | Partial/full receipt, stock-on-approval-only, over-receive guard, accumulation |
| `ProcurementLegalAiRegressionTests.cs` | Rule versioning, deprecated-not-used, finding history, AI advisory + disclaimer + no API-key leak |

## Coverage matrix
| Module / concern | Existing tests (observed) | Phase 39 target file | Status |
|---|---|---|---|
| MESC Catalog | MescCatalogTests, MescWebUiTests | EndToEnd (consumed) | covered |
| Orders / Inventory Control | OrdersTests, OrdersWebUiTests, ApiAuthorizationTests (need→indent, shortage→indent) | EndToEnd, Inventory | partial → extend |
| Material Needs | ApiAuthorizationTests (create/submit/approve/convert) | EndToEnd | covered |
| Shortage Alerts | ApiAuthorizationTests (detect/convert) | EndToEnd | covered |
| Indent / Purchase Request | IndentTests, IndentModuleTests, IndentWebUiTests | EndToEnd, WorkflowInbox | partial → extend send-to-purchase |
| Purchase File | PurchaseFileModuleTests, PurchaseFileUiTests | EndToEnd, Document | covered |
| Workflow / Inbox | WorkflowModuleTests, WorkflowActionMatrixTests, InboxWebUiTests | WorkflowInbox | **gap**: assert Instance+Step+InboxTask on send |
| File Repository / Documents | FileStorageServiceTests | Document | partial → per-module upload/download |
| Supplier Management | SupplierTests, SupplierWebUiTests, ApiAuthorizationTests | EndToEnd | covered |
| Inquiry / RFQ | InquiryTests, InquiryWebUiTests, ApiAuthorizationTests (create/send/quote/comparison) | EndToEnd | covered |
| Tender | TenderDomainTests, ApiAuthorizationTests (create/report/docs) | EndToEnd, Authorization | partial → publish/bid/evaluate/winner |
| Commission Sessions | TenderCommissionSessionTests, CommissionWebUiTests, ApiAuthorizationTests | EndToEnd, Authorization | partial → minutes/decision/approve |
| Tender/Commission Reports | ReportGeneratorTests, ApiAuthorizationTests (tender pdf) | Document | partial → save-to-file + link |
| Contract Management | ContractTests, ContractModuleTests, ContractWebUiTests | EndToEnd, Authorization | partial → template/submit/approve/sign |
| Purchase Order | PurchaseOrderTests, PurchaseOrderNumberServiceTests | EndToEnd, Authorization | partial → submit/approve/issue/pdf |
| Warehouse Receipt | WarehouseReceiptTests | EndToEnd, Inventory | partial → from-PO/submit/approve |
| StockBalance | (via WarehouseReceiptTests) | Inventory | **gap**: approval-only + accumulation |
| InventoryTransaction | (via WarehouseReceiptTests) | Inventory | **gap**: created on approval |
| Versioned Legal Rules | LegalRuleTests | LegalAi | partial → active-immutable, deprecated-not-used |
| LegalDocument storage | LegalDocumentStorageServiceTests | Document, LegalAi | partial → soft-delete visibility |
| Procurement Rule Evaluation | LegalRuleTests | LegalAi | partial → findings persisted + version ref |
| AiCore Provider Integration | AiFoundationTests, AiAsyncContractsTests, AiJobQueueIntegrationTests, AiCoreCallbackTests, AiJobSignalRTests, PurchaseFileAiContextBuilderTests | LegalAi | covered → add no-key-leak + advisory |
| Reporting foundation | ReportGeneratorTests | Document | partial |
| Admin foundation | AdminWebUiTests | Authorization | covered |
| Persian Date / Localization | PersianDateUiTests | (UI pass) | covered |
| Security / JWT / Permissions | PermissionPolicyTests, WebAuthenticationTests, ApiAuthorizationTests | Authorization | covered → extend new modules |
| Migration / seed safety | IdentitySeedDataTests | (new seed-safety tests) | **gap** |
| Lifecycle read model | ProcurementLifecycleEndpointTests | EndToEnd (`GET /api/purchase-files/{id}/lifecycle`) | endpoint + web tab added; broaden E2E chain assertions |

## Authorization 403 matrix to assert (ProcurementAuthorizationRegressionTests)
Tender: View / Create / Publish. Commission: View / Create / Approve. Contract: View / Create / Approve / Sign. PurchaseOrder: View / Create / Issue. WarehouseReceipt: View / Create / Approve. LegalRules: View / Manage / Evaluate. AI: ProviderManage / ProviderTest / (AnalyzePurchaseFile|AgentUse). Plus: forged `CreatedByUserId`/`ActingUserId`/`IsAdmin` ignored.

## Known gaps to close before sign-off
1. WorkflowInstance/Step/InboxTask assertions on indent send.
2. Inventory: stock-on-approval-only, over-receive guard, multi-partial accumulation, transaction-on-approval.
3. Per-module document upload/download/soft-delete + report save-to-file linkage.
4. Legal rule versioning immutability + deprecated-not-used + finding version reference.
5. Broaden lifecycle assertions beyond the basic endpoint regression to cover related Inquiry/Tender/Commission/Contract/PO/WarehouseReceipt chains.
6. Seed-safety/migration idempotency tests.
