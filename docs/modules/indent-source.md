# Indent Source Model

از فاز ۳۰.۱، هر Indent یک منبع رسمی دارد تا مشخص باشد تقاضای خرید از کدام مسیر ایجاد شده است.

## Source Types

- `Manual`: تقاضای دستی
- `DirectPurchase`: خرید مستقیم
- `SystemGenerated`: تقاضای سیستمی
- `MaterialNeed`: ایجاد شده از نیاز کالا
- `ShortageAlert`: ایجاد شده از هشدار کمبود
- `ApplicantNeed`: نیاز متقاضی
- `Other`: سایر

## قواعد

- Indentهای دستی مقدار `Manual` می‌گیرند.
- Indentهای نوع خرید مستقیم مقدار `DirectPurchase` می‌گیرند.
- Indentهای سیستمی مقدار `SystemGenerated` می‌گیرند.
- Indentهای ساخته‌شده از MaterialNeed مقدار `MaterialNeed` و `SourceMaterialNeedId` دارند.
- Indentهای ساخته‌شده از ShortageAlert مقدار `ShortageAlert` و `SourceShortageAlertId` دارند.
- Indentهای seed یا داده‌های قدیمی با مقدار امن `Manual` backfill می‌شوند.

## نمایش در UI

Web از فیلدهای زیر استفاده می‌کند:

- `SourceType`
- `SourceDescription`
- `SourceReferenceId`
- `SourceDisplayText`

برای MaterialNeed، لینک به `/orders/material-needs/{id}` نمایش داده می‌شود. برای ShortageAlert، لینک به صفحه هشدارهای کمبود نمایش داده می‌شود.

## Unit of Measure consistency

`MescItem` اکنون `UnitOfMeasureId` رسمی دارد. متن `UnitOfMeasure` برای نمایش و سازگاری نگه داشته شده، اما مسیرهای داخلی جدید برای ایجاد `MaterialNeed`، `ShortageAlert` و `IndentItem` از `UnitOfMeasureId` استفاده می‌کنند.
