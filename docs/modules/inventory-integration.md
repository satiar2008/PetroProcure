# Inventory Integration

این سند اتصال اولیه موجودی کالا با رسید انبار را توضیح می‌دهد.

## محدوده فاز

در این فاز فقط دریافت کالا از سفارش خرید پیاده‌سازی شده است. موارد زیر عمداً خارج از محدوده هستند:

- Finance / Payment
- Invoice Matching
- Accounting
- Advanced Delivery Tracking

## رفتار موجودی

با تأیید Warehouse Receipt:

1. `PurchaseOrderItem.ReceivedQuantity` افزایش می‌یابد.
2. `PurchaseOrderItem.RemainingQuantity` کاهش می‌یابد.
3. اگر مانده همه اقلام صفر شود، سفارش خرید `FullyReceived` می‌شود.
4. در غیر این صورت سفارش خرید `PartiallyReceived` می‌شود.
5. `StockBalance` موجود در ماژول Orders برای همان `MescItemId` و `WarehouseId` افزایش می‌یابد.
6. یک `InventoryTransaction` با نوع `Receipt` ایجاد می‌شود.

## شماره‌گذاری

- Warehouse Receipt: `WR-YYYY-000001`
- Inventory Transaction: `ITX-YYYY-000001`

هر دو شماره به‌صورت sequence سالانه و transaction-safe تولید می‌شوند.

## APIهای موجودی

- `GET /api/inventory/stock-balances`
- `GET /api/inventory/transactions`

## نکات آینده

- خروج کالا، رزرو، اصلاح موجودی و cycle count باید در فازهای بعدی با مجوزهای جداگانه پیاده‌سازی شوند.
- گزارش‌های تحلیلی موجودی باید از تراکنش‌ها و StockBalance تولید شوند.

