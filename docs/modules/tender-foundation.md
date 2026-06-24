# Tender Foundation

ماژول مناقصه، فرآیند رسمی تأمین کالا/خدمت را برای پرونده‌های خرید مدیریت می‌کند. هر مناقصه الزاماً به یک `PurchaseFile` متصل است و در صورت نیاز می‌تواند از نتیجه یک Inquiry/RFQ ساخته شود.

## جایگاه در معماری

- `PurchaseFile` همچنان aggregate مرکزی کسب‌وکار است.
- Tender اقلام خود را معمولاً از `PurchaseFileItem` کپی می‌کند.
- شرح عمومی، شرح اختصاصی، کد MESC و مقدار در `TenderItem` به‌صورت snapshot ذخیره می‌شود.
- شرکت‌کنندگان Tender از ماژول Supplier انتخاب می‌شوند.
- Supplierهای غیرفعال یا blacklisted به‌صورت پیش‌فرض مجاز نیستند.
- تصمیم نهایی برنده فقط توسط انسان/کمیسیون انجام می‌شود؛ AI در آینده فقط نقش پیشنهاددهنده و هشداردهنده خواهد داشت.

## موجودیت‌ها

- Tender
- TenderItem
- TenderParticipant
- TenderStage
- TenderBid
- TenderBidItem
- TenderEvaluation
- TenderDecision
- TenderDocument

## چرخه وضعیت

وضعیت‌های پایه:

- Draft
- ReadyToPublish
- Published
- ReceivingBids
- UnderQualification
- UnderTechnicalEvaluation
- UnderCommercialEvaluation
- UnderFinalReview
- WinnerSelected
- Closed
- Cancelled

Tenderهای بسته‌شده یا لغوشده read-only هستند.

## شماره‌گذاری

فرمت شماره مناقصه:

`TND-YYYY-000001`

Sequence به‌صورت سالانه افزایش پیدا می‌کند و در زمان تولید شماره با transaction محافظت می‌شود.

## Permissionها

- Tender.View
- Tender.Create
- Tender.Edit
- Tender.Publish
- Tender.Cancel
- Tender.ManageItems
- Tender.ManageParticipants
- Tender.ReceiveBid
- Tender.EvaluateQualification
- Tender.EvaluateTechnical
- Tender.EvaluateCommercial
- Tender.CompareBids
- Tender.SelectWinner
- Tender.Close
- Tender.ManageDocuments

## APIهای پایه

- `GET /api/tenders`
- `GET /api/tenders/{id}`
- `GET /api/tenders/by-number/{tenderNumber}`
- `GET /api/purchase-files/{purchaseFileId}/tenders`
- `POST /api/tenders`
- `POST /api/tenders/from-purchase-file/{purchaseFileId}`
- `POST /api/tenders/from-inquiry/{inquiryId}`
- `PUT /api/tenders/{id}`
- `POST /api/tenders/{id}/publish`
- `POST /api/tenders/{id}/cancel`
- `POST /api/tenders/{id}/close`
- `GET /api/tenders/{id}/comparison`
- `POST /api/tenders/{id}/select-winner`

## صفحات Web

- `/purchase/tenders`
- `/purchase/tenders/create`
- `/purchase/tenders/from-purchase-file/{purchaseFileId}`
- `/purchase/tenders/{id}`

## توسعه‌های آینده

- Tender Commission Sessions
- گزارش رسمی Tender Summary
- گزارش Tender Comparison
- گزارش Tender Winner Decision
- اتصال به Contract
- اتصال به Purchase Order
- AI completeness check برای اسناد مناقصه
- AI bid comparison assistant با الزام تأیید انسانی
