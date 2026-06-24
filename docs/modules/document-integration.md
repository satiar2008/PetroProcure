# اتصال اسناد مناقصه و کمیسیون به مخزن پرونده خرید

Phase 33 اسناد مناقصه و پیوست‌های کمیسیون را به File Repository مرکزی پرونده خرید متصل کرد.

## اصول ذخیره‌سازی

- فایل فیزیکی از طریق `IFileStorageService` در ساختار استاندارد `PurchaseFiles/{year}/{fileNumber}` ذخیره می‌شود.
- مسیر مطلق در پایگاه داده ذخیره نمی‌شود؛ فقط مسیر نسبی نگهداری می‌شود.
- نام فایل ذخیره‌شده امن تولید می‌شود و نام اصلی فقط برای نمایش حفظ می‌شود.
- حذف اسناد به صورت soft delete انجام می‌شود.
- گزارش‌های تولیدشده نیز به همین مخزن برگشت داده می‌شوند.

## Endpointهای اسناد مناقصه

- `POST /api/tenders/{id}/documents/upload`
- `GET /api/tenders/{id}/documents`
- `GET /api/tenders/{id}/documents/{documentId}/download`
- `DELETE /api/tenders/{id}/documents/{documentId}`

## Endpointهای پیوست کمیسیون

- `POST /api/commission/sessions/{id}/attachments/upload`
- `GET /api/commission/sessions/{id}/attachments`
- `GET /api/commission/sessions/{id}/attachments/{attachmentId}/download`
- `DELETE /api/commission/sessions/{id}/attachments/{attachmentId}`

## مجوزها

- اسناد مناقصه: `Tender.View` برای مشاهده/دانلود و `Tender.ManageDocuments` برای بارگذاری/حذف.
- پیوست‌های کمیسیون: `Commission.View` برای مشاهده/دانلود و `Commission.ManageDocuments` برای بارگذاری/حذف.

## محدودیت‌های تولیدی باقی‌مانده

- اسکن آنتی‌ویروس واقعی هنوز باید با سرویس سازمانی یا ClamAV جایگزین NoOp شود.
- سیاست نگهداری و پاکسازی فایل‌های orphan باید در Worker فعال شود.
- طبقه‌بندی محرمانگی اسناد و watermark رسمی می‌تواند در فازهای بعدی اضافه شود.
