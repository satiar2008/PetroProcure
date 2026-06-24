# گزارش‌های کمیسیون مناقصه

در Phase 33 گزارش‌های کمیسیون مناقصه برای چاپ، خروجی PDF و ذخیره در مخزن اسناد پرونده خرید آماده شد.

## گزارش‌های موجود

- `CommissionSessionMinutesReport`: صورتجلسه کامل شامل اعضا، دستورجلسه، متن صورتجلسه‌ها و تصمیمات.
- `CommissionDecisionReport`: گزارش مستقل برای هر تصمیم کمیسیون.

## API

- `GET /api/commission/sessions/{id}/reports/minutes/pdf`
- `POST /api/commission/sessions/{id}/reports/minutes/save-to-file`
- `GET /api/commission/sessions/{id}/reports/decision/{decisionId}/pdf`
- `POST /api/commission/sessions/{id}/reports/decision/{decisionId}/save-to-file`

## مجوزها

- خروجی و ذخیره گزارش‌های کمیسیون با `Commission.ReportExport` کنترل می‌شود.
- UI نمایش گزارش را برای کاربران دارای `Commission.ReportView` یا `Commission.ReportExport` فعال می‌کند.

## یادآوری

گزارش تصمیم کمیسیون، ثبت و چاپ تصمیم انسانی است؛ سیستم هیچ تصمیم نهایی تجاری یا حقوقی را خودکار اتخاذ نمی‌کند.
