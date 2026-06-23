# PetroProcure

PetroProcure سامانه مدیریت تدارکات پالایشگاهی است که تمرکز اصلی آن بر مدیریت پرونده خرید است. این مخزن در حال حاضر فقط شامل مستندات اولیه و اسکلت راهکار ASP.NET Aspire است و هنوز موجودیت‌های تجاری، منطق دامنه، گزارش‌ها یا قابلیت‌های هوش مصنوعی پیاده‌سازی نشده‌اند.

## هدف پروژه

هدف محصول، ایجاد یک بستر سازمانی برای ثبت، پیگیری و گزارش‌گیری پرونده‌های خرید پالایشگاهی است. در فازهای بعدی، پرونده خرید، اقلام با کد MESC، اسناد و پیوست‌ها، گردش کار واحدها، گزارش‌های رسمی DevExpress و AI Agent به صورت مرحله‌ای اضافه می‌شوند.

رابط کاربری از ابتدا برای پشتیبانی از فارسی و انگلیسی آماده شده است. زبان پیش‌فرض فارسی است و امکان تغییر به انگلیسی از صفحه اصلی وجود دارد.

## ساختار راهکار

```text
docs/
  مستندات چشم‌انداز، معماری، دامنه، گردش کار، MESC، Indent، فایل‌ها، گزارش‌گیری و AI

src/
  PetroProcure.AppHost/          ارکستریشن Aspire برای Web، Api و SQL Server
  PetroProcure.ServiceDefaults/  تنظیمات مشترک Aspire، سلامت، OpenTelemetry و Service Discovery
  PetroProcure.Web/              Blazor Web App با پشتیبانی اولیه فارسی و انگلیسی
  PetroProcure.Api/              ASP.NET Core Web API
  PetroProcure.Domain/           لایه دامنه، بدون وابستگی به پروژه‌های دیگر
  PetroProcure.Application/      لایه کاربرد، وابسته به Domain و Contracts
  PetroProcure.Infrastructure/   زیرساخت و آماده برای SQL Server و EF Core
  PetroProcure.Contracts/        قراردادهای مشترک بین Web و Api
  PetroProcure.Reporting/        فضای ایزوله برای DevExpress Reports در فازهای بعدی
  PetroProcure.AI/               فضای ایزوله برای AI Agent و Providerهای آینده
  PetroProcure.Worker/           سرویس پس‌زمینه آماده برای پردازش‌های آتی

tests/
  PetroProcure.UnitTests/
  PetroProcure.IntegrationTests/
  PetroProcure.ArchitectureTests/
```

## اجرای پروژه با Aspire

ابتدا وابستگی‌ها را بازیابی و راهکار را build کنید:

```bash
dotnet restore
dotnet build
```

برای اجرای محیط Aspire:

```bash
dotnet run --project src/PetroProcure.AppHost/PetroProcure.AppHost.csproj
```

AppHost پروژه‌های Web و Api را اجرا می‌کند و یک منبع SQL Server با نام `sql` و دیتابیس `petroprocuredb` را برای استفاده آینده آماده می‌کند.

## قوانین توسعه

- تا زمان شروع فاز دامنه، هیچ موجودیت تجاری در پروژه‌ها اضافه نشود.
- `PetroProcure.Domain` نباید به هیچ پروژه کاربردی یا زیرساختی وابسته باشد.
- منطق تجاری نباید در Blazor UI پیاده‌سازی شود.
- قراردادهای مشترک Web و Api باید در `PetroProcure.Contracts` قرار بگیرند.
- کدهای مربوط به EF Core و SQL Server باید در `PetroProcure.Infrastructure` متمرکز بمانند.
- گزارش‌های رسمی باید در آینده از مسیر `PetroProcure.Reporting` و DevExpress Reports توسعه داده شوند.
- قابلیت‌های AI باید در `PetroProcure.AI` ایزوله بمانند و به صورت مستقیم مالک داده‌های دامنه نشوند.
- Worker فقط برای پردازش‌های پس‌زمینه استفاده شود و منطق اصلی پرونده خرید در آن پخش نشود.
- هر تغییر معماری مهم باید در مستندات `docs/` منعکس شود.
