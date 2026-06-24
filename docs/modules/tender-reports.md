# گزارش‌های مناقصه

در Phase 33 زیرساخت گزارش‌های رسمی مناقصه به DevExpress Reports و مخزن اسناد پرونده خرید متصل شد.

## گزارش‌های موجود

- `TenderSummaryReport`: خلاصه مناقصه، پرونده خرید، استعلام مبنا، شرکت‌کنندگان و اقلام گروه‌بندی‌شده بر اساس MESC.
- `TenderComparisonReport`: مقایسه پیشنهادهای تأمین‌کنندگان، امتیازها، شرایط پرداخت/تحویل و وضعیت فنی اقلام.
- `TenderWinnerDecisionReport`: گزارش تصمیم انتخاب برنده بر اساس داده‌های ثبت‌شده در مناقصه.

## API

- `GET /api/tenders/{id}/reports/summary/pdf`
- `POST /api/tenders/{id}/reports/summary/save-to-file`
- `GET /api/tenders/{id}/reports/comparison/pdf`
- `POST /api/tenders/{id}/reports/comparison/save-to-file`
- `GET /api/tenders/{id}/reports/winner-decision/pdf`
- `POST /api/tenders/{id}/reports/winner-decision/save-to-file`

## مجوزها

- مشاهده/خروجی PDF گزارش‌های مناقصه با `Tender.ReportExport` کنترل می‌شود.
- در UI، نمایش کارت‌های گزارش با `Tender.ReportView` یا `Tender.ReportExport` انجام می‌شود.

## نکته حاکمیتی

گزارش‌ها ابزار رسمی چاپ، بایگانی و بررسی هستند؛ انتخاب برنده و تصمیم نهایی همچنان انسانی و در اختیار کمیسیون/کاربر مجاز است.
