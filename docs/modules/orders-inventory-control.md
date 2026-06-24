# Orders & Inventory Control

ماژول سفارشات و کنترل موجودی نقطه ورود عملیاتی فرآیند تأمین کالا در PetroProcure است. این واحد نیازهای کالا، وضعیت برنامه‌ریزی موجودی و هشدارهای کمبود را مدیریت می‌کند و خروجی اصلی آن Indent / Purchase Request است.

## جایگاه در فرآیند

1. متقاضی یا واحد سفارشات نیاز کالا را شناسایی می‌کند.
2. سفارشات و کنترل موجودی وضعیت کالا، موجودی و نقطه سفارش را بررسی می‌کند.
3. نیاز کالا یا هشدار کمبود تأیید می‌شود.
4. نیاز یا کمبود به Indent تبدیل می‌شود.
5. Indent پس از تأیید به واحد خرید ارسال می‌شود.
6. واحد خرید از روی Indent پرونده خرید ایجاد می‌کند.

## مرزها

- Orders & Inventory Control مسئول برنامه‌ریزی، نیازسنجی، کنترل موجودی خواندنی/برنامه‌ریزی و ساخت Indent است.
- Warehouse مسئول عملیات فیزیکی دریافت، خروج و تراکنش‌های واقعی موجودی است.
- Purchase Department مسئول PurchaseFile، Supplier، Inquiry/RFQ و مراحل بعدی خرید است.
- Applicant می‌تواند نیاز کالا ثبت کند، اما تنظیمات کنترل موجودی را مدیریت نمی‌کند.

## موجودیت‌ها

- `InventoryControlItem`: تنظیمات کنترل موجودی برای یک MESC Item.
- `StockBalance`: نمای برنامه‌ریزی موجودی، شامل موجودی قابل دسترس، رزروشده و در راه.
- `MaterialNeed`: نیاز کالا با snapshot اطلاعات MESC.
- `ShortageAlert`: هشدار کمبود بر اساس موجودی و نقطه سفارش.

## قواعد مهم

- نیاز کالا بدون MESC Item معتبر ایجاد نمی‌شود.
- نیاز Draft قابل ارسال است.
- نیاز Submitted یا UnderReview قابل تأیید/رد است.
- فقط نیاز Approved قابل تبدیل به Indent است.
- نیاز Rejected قابل تبدیل به Indent نیست.
- هشدار کمبود Open یا InProgress قابل تبدیل به Indent است.
- Snapshot کد MESC، گروه عمومی، شرح عمومی، شرح اختصاصی و واحد در نیاز/کمبود نگهداری می‌شود.
- UserId، ActingUserId و IsAdmin از کلاینت پذیرفته نمی‌شود و از Claims خوانده می‌شود.

## مجوزها

- `Orders.ViewDashboard`
- `Orders.ViewInventory`
- `Orders.ManageInventoryControl`
- `Orders.CreateMaterialNeed`
- `Orders.ReviewMaterialNeed`
- `Orders.ApproveMaterialNeed`
- `Orders.ConvertNeedToIndent`
- `Orders.ConvertShortageToIndent`
- `Orders.ManageShortageAlerts`

## API

- `GET /api/orders/dashboard`
- `GET /api/orders/inventory-control`
- `PUT /api/orders/inventory-control/{id}`
- `GET /api/orders/stock-balances`
- `GET /api/orders/material-needs`
- `GET /api/orders/material-needs/{id}`
- `POST /api/orders/material-needs`
- `POST /api/orders/material-needs/{id}/submit`
- `POST /api/orders/material-needs/{id}/review`
- `POST /api/orders/material-needs/{id}/approve`
- `POST /api/orders/material-needs/{id}/reject`
- `POST /api/orders/material-needs/{id}/convert-to-indent`
- `GET /api/orders/shortage-alerts`
- `POST /api/orders/shortage-alerts/detect`
- `POST /api/orders/shortage-alerts/{id}/convert-to-indent`
- `POST /api/orders/shortage-alerts/{id}/resolve`

## صفحات Web

- `/orders`
- `/orders/inventory-control`
- `/orders/material-needs`
- `/orders/material-needs/create`
- `/orders/material-needs/{id}`
- `/orders/shortage-alerts`

## موارد باقی‌مانده

- اتصال کامل به ماژول Warehouse برای تراکنش‌های واقعی موجودی.
- فرم‌های پیشرفته‌تر انتخاب واحد متقاضی و فیلترهای کامل‌تر.
- گزارش رسمی Material Need و Shortage Alert.
- نمایش source در Indent اکنون رسمی شده و برای MaterialNeed و ShortageAlert لینک مرجع نگهداری می‌شود.
- MESC Item اکنون علاوه بر متن واحد، `UnitOfMeasureId` رسمی دارد و تبدیل نیاز/کمبود به Indent از شناسه واحد استفاده می‌کند.
