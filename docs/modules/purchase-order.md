# Purchase Order Foundation

ماژول سفارش خرید، سند رسمی خرید را بعد از قرارداد تأیید/امضاشده یا در حالت خرید مستقیم از پرونده خرید ایجاد می‌کند.

## هدف کسب‌وکار

Purchase Order همیشه به یک PurchaseFile و یک Supplier متصل است. در حالت معمول از Contract ساخته می‌شود و اقلام، قیمت‌ها، شرایط پرداخت/تحویل و مبالغ را به‌صورت snapshot کپی می‌کند. برای خرید مستقیم، امکان ایجاد از PurchaseFile با انتخاب Supplier فراهم است.

## ارتباط‌ها

- PurchaseFile: مرکز پرونده و محل ذخیره اسناد سفارش خرید.
- Supplier: تأمین‌کننده سفارش.
- Contract: اختیاری؛ برای سفارش‌های مبتنی بر قرارداد.
- Tender/TenderBid: اختیاری و معمولاً از مسیر قرارداد منتقل می‌شود.
- Warehouse Receipt آینده: رسید انبار بعداً در برابر اقلام PurchaseOrder ثبت می‌شود.

## چرخه وضعیت

Draft → UnderReview → Approved → Issued → PartiallyReceived/FullyReceived → Completed

وضعیت‌های Cancelled و Archived نیز برای توقف/بایگانی وجود دارد.

## قوانین کلیدی

- شماره سفارش فرمت `PO-YYYY-000001` دارد و در هر سال افزایشی است.
- بدون Supplier و حداقل یک item، سفارش قابل submit/issue نیست.
- صدور سفارش فقط بعد از approval مجاز است.
- اقلام snapshot شرح عمومی/اختصاصی MESC، واحد، مقدار و قیمت را نگه می‌دارند.
- سفارش‌های Issued/Completed/Cancelled/Archived فقط با دسترسی اصلاح ادمین قابل تغییر هستند.
- UserId از کلاینت پذیرفته نمی‌شود و از Claims خوانده می‌شود.

## مجوزها

- `PurchaseOrder.View`
- `PurchaseOrder.Create`
- `PurchaseOrder.Edit`
- `PurchaseOrder.Submit`
- `PurchaseOrder.Approve`
- `PurchaseOrder.Reject`
- `PurchaseOrder.Issue`
- `PurchaseOrder.Cancel`
- `PurchaseOrder.ManageItems`
- `PurchaseOrder.ManageDocuments`
- `PurchaseOrder.ReportView`
- `PurchaseOrder.ReportExport`

## API endpoints

- `GET /api/purchase-orders`
- `GET /api/purchase-orders/{id}`
- `GET /api/purchase-orders/by-number/{purchaseOrderNumber}`
- `GET /api/purchase-files/{purchaseFileId}/purchase-orders`
- `GET /api/suppliers/{supplierId}/purchase-orders`
- `GET /api/contracts/{contractId}/purchase-orders`
- `POST /api/purchase-orders`
- `POST /api/purchase-orders/from-contract/{contractId}`
- `POST /api/purchase-orders/from-purchase-file/{purchaseFileId}`
- `PUT /api/purchase-orders/{id}`
- `POST /api/purchase-orders/{id}/submit`
- `POST /api/purchase-orders/{id}/approve`
- `POST /api/purchase-orders/{id}/reject`
- `POST /api/purchase-orders/{id}/issue`
- `POST /api/purchase-orders/{id}/cancel`
- `GET/POST/DELETE /api/purchase-orders/{id}/items`
- `GET/POST/DELETE /api/purchase-orders/{id}/documents`
- `GET /api/purchase-orders/{id}/reports/purchase-order/pdf`
- `POST /api/purchase-orders/{id}/reports/purchase-order/save-to-file`

## Web pages

- `/purchase/purchase-orders`
- `/purchase/purchase-orders/create`
- `/purchase/purchase-orders/from-contract/{contractId}`
- `/purchase/purchase-orders/from-purchase-file/{purchaseFileId}`
- `/purchase/purchase-orders/{id}`
- `/warehouse/purchase-orders` برای نمای view-only سفارش‌های مرتبط با دریافت آینده.

## گزارش

`PurchaseOrderReport` شامل شماره سفارش، پرونده خرید، قرارداد، تأمین‌کننده، تاریخ‌ها، شرایط پرداخت/تحویل، مبالغ و اقلام گروه‌بندی‌شده براساس MESC است. PDF قابل ذخیره در مخزن اسناد PurchaseFile است.

## کارهای آینده

- Warehouse Receipt عملیاتی و به‌روزرسانی ReceivedQuantity/RemainingQuantity.
- اتصال مالی/پرداخت.
- Delivery Tracking.
- کنترل دقیق‌تر اصلاح ادمین برای سفارش‌های صادرشده.
