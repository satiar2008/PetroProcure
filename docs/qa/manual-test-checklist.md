# Manual Test Checklist — End-to-End Procurement (Phase 39)

A human-runnable checklist to validate the full lifecycle in a running environment (Aspire AppHost or API+Web). Use alongside `end-to-end-procurement-flow.md`.

## 1. Test roles / users
Create (or reuse the bootstrap admin) users covering these role groups. A single SystemAdmin can do everything, but to validate authorization use **scoped** users:

| Role group | Purpose | Key permissions |
|---|---|---|
| Orders/Inventory clerk | material needs, shortages, indents | `Orders.CreateMaterialNeed`, `Orders.ApproveMaterialNeed`, `Orders.ConvertNeedToIndent`, `Orders.ManageShortageAlerts`, `Indent.Create` |
| Indent approver | submit/approve/send indent | `Indent.Create`, `Indent.Approve`, `Indent.SendToPurchase` |
| Purchase officer | purchase file, inquiry, tender | `PurchaseFile.Create`, `PurchaseFile.View`, `PurchaseFile.Edit`, `Inquiry.Create`, `Inquiry.Send`, `Inquiry.ReceiveQuote`, `Inquiry.CompareQuotes`, `Tender.Create`, `Tender.Publish` |
| Commission secretary | commission sessions | `Commission.Create`, `Commission.ManageMembers`, `Commission.ManageAgenda`, `Commission.ManageMinutes`, `Commission.ManageDecisions`, `Commission.Approve`, `Commission.ReportExport` |
| Contracts officer | contracts | `Contract.Create`, `Contract.Submit`, `Contract.Approve`, `Contract.Sign`, `Contract.ReportExport` |
| Procurement (PO) | purchase orders | `PurchaseOrder.Create`, `PurchaseOrder.Submit`, `PurchaseOrder.Approve`, `PurchaseOrder.Issue`, `PurchaseOrder.ReportExport` |
| Warehouse keeper | receipts, stock | `WarehouseReceipt.Create`, `WarehouseReceipt.Submit`, `WarehouseReceipt.Approve`, `Inventory.ViewStockBalance`, `Inventory.ViewTransactions`, `WarehouseReceipt.ReportExport` |
| Legal officer | rules, legal docs | `ProcurementRule.View`, `ProcurementRule.Manage`, `ProcurementRule.Evaluate` |
| AI admin | provider config | `Ai.ProviderManage`, `Ai.ProviderTest`, `Ai.AnalyzePurchaseFile` |

> Verify exact permission constant names in `ApplicationPermissions`. The claim values are dotted strings (e.g. `PurchaseFile.Edit`).

## 2. Pre-conditions
- Database migrated; bootstrap admin enabled (or seeded users present).
- MESC catalog has at least one general group + item (e.g. seeded Pipe / Gate valve).
- A default warehouse exists.
- AI provider in **Mock** or `SyncAiCoreDirect` mode (so analysis completes without a live AiCore).

## 3. Step-by-step (record PASS/FAIL + notes)

| # | Action (UI page) | Expected status | Expected document | Expected inventory | Expected legal/AI |
|---|---|---|---|---|---|
| 1 | Orders → Material Needs → Create | Draft | — | — | — |
| 2 | Submit | Submitted | — | — | — |
| 3 | Approve (as approver) | Approved | — | — | — |
| 4 | Convert to Indent | Indent created | — | — | — |
| 5 | Indent detail | shows source = Material Need, MESC items | — | — | — |
| 6–8 | Submit → Approve → Send to Purchase | SentToPurchase | — | — | — |
| 9 | Purchase Inbox (کارتابل واحد) | InboxTask visible | — | — | — |
| 10–11 | Open task → Create Purchase File | PurchaseFile Draft | Indent attached | — | — |
| 12 | Purchase File → items tab | items grouped by MESC, counts match | — | — | — |
| 13–17 | Inquiry from PF → add supplier → send → quote → comparison | Inquiry Sent; comparison shows suppliers | — | — | — |
| 18–23 | Tender create → participant → publish → bid → evaluate → winner | Published → winner Selected | — | — | — |
| 24–29 | Commission from tender → member → agenda → minute → decision → complete → approve | Approved | — | — | — |
| 30 | Save commission/tender report to PF | — | report in PF documents | — | — |
| 31–35 | Contract from tender/PF → template → submit → approve → sign | Signed | — | — | — |
| 36 | Contract PDF → save to PF | — | contract PDF in PF documents | — | — |
| 37–40 | PO from contract → submit → approve → issue | Issued | — | — | — |
| 41 | PO PDF → save to PF | — | PO PDF in PF documents | — | — |
| 42–43 | Warehouse: receipt from issued PO → submit | Submitted | — | **no** stock change | — |
| 44 | Approve receipt | Approved | — | stock += received; transaction created | — |
| 45–46 | PO detail | ReceivedQty↑, RemainingQty↓; PartiallyReceived/FullyReceived | — | — | — |
| 47–48 | Warehouse → Stock balances / Inventory transactions | balance increased; receipt transaction listed | — | — | — |
| 49 | Receipt PDF → save to PF | — | receipt PDF in PF documents | — | — |
| 50 | Run legal rule evaluation on PF/Tender/Contract | — | — | — | findings stored with legal references |
| 51 | Run AI advisory on PF (خلاصه پرونده) | — | — | — | advisory result + disclaimer; no business change |
| 52 | Purchase File → «گردش کامل خرید» tab | full chain cards/timeline render | — | — | — |

## 4. Negative / authorization spot checks
- A user missing `Tender.Publish` cannot see/use the Publish button and the API returns 403.
- A user missing `Contract.Sign` cannot sign; API 403.
- A user missing `WarehouseReceipt.Approve` cannot approve; API 403.
- AiCore settings page never displays the API key value (only “configured: yes/no”).
- Submitting a forged `CreatedByUserId`/`ActingUserId`/`IsAdmin` in any create body is ignored; identity comes from the token.

## 5. Localization / UX spot checks
- All dates render via Persian `DateDisplay` (no raw UTC).
- No raw GUIDs shown to users (names/numbers instead).
- Empty states are friendly Persian messages, not blank tables.
- Loading/error use `DataStateView` + `RetryPanel`.
- `Status=` (empty enum) in a list query does not crash.
