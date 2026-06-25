# Warehouse Receipt

ماژول رسید انبار دریافت فیزیکی کالا را در برابر سفارش خرید صادرشده ثبت می‌کند.

## قواعد اصلی

- رسید انبار فقط برای Purchase Order با وضعیت `Issued` یا `PartiallyReceived` ایجاد می‌شود.
- هر رسید دارای شماره یکتا با قالب `WR-YYYY-000001` است.
- اقلام رسید از اقلام سفارش خرید snapshot می‌شوند.
- مقدار دریافتی هر قلم نمی‌تواند از مانده سفارش بیشتر باشد.
- رسید در وضعیت `Draft` قابل ویرایش است.
- با `Submit` رسید برای تأیید ارسال می‌شود.
- با `Approve`:
  - مقدار دریافت‌شده روی قلم سفارش خرید اعمال می‌شود.
  - وضعیت سفارش خرید به `PartiallyReceived` یا `FullyReceived` تغییر می‌کند.
  - موجودی انبار به‌روزرسانی می‌شود.
  - تراکنش موجودی از نوع `Receipt` ثبت می‌شود.
- رسید تأییدشده یا لغوشده read-only است.

## APIهای اصلی

- `GET /api/warehouse-receipts`
- `GET /api/warehouse-receipts/{id}`
- `POST /api/warehouse-receipts/from-purchase-order/{purchaseOrderId}`
- `POST /api/warehouse-receipts/{id}/submit`
- `POST /api/warehouse-receipts/{id}/approve`
- `POST /api/warehouse-receipts/{id}/cancel`
- `GET /api/purchase-orders/waiting-for-receipt`

## UI

صفحات عملیاتی در پنل انبار:

- `/warehouse`
- `/warehouse/purchase-orders-waiting`
- `/warehouse/receipts`
- `/warehouse/receipts/from-purchase-order/{purchaseOrderId}`
- `/warehouse/receipts/{id}`

