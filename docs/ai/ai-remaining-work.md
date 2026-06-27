# PetroProcure — قسمت‌های نیازمند توسعه و تکمیل

تاریخ: ۱۴۰۵/۰۴ — پس از تکمیل فازهای AI-00 تا AI-09.
این سند فقط «کارهای باقی‌مانده» را فهرست می‌کند (نه آنچه انجام شده). اولویت از بالا به پایین.

## وضعیت فازهای AI
- AI-00 کشف، AI-01 مستندسازی، AI-02 قراردادها، AI-03 صف کار، AI-04 Context Builder، AI-05 Endpointها، AI-06 Worker Dispatcher، **AI-07 Callback**، **AI-08 UI پولینگ**، **AI-09 SignalR** → انجام شده.
- AI-10، AI-11، AI-12 → باقی‌مانده (پایین‌تر شرح داده شده).

---

## ۱. بحرانی — تا این‌ها نباشد، مسیر پیش‌فرض async کار نمی‌کند

**AI-11 — تکمیل سمت AiCore (خارج از این مخزن):** AiCore هنوز این‌ها را ندارد و سمت PetroProcure منتظر آن‌هاست (submit client + گیرنده callback + پولینگ + SignalR همگی آماده‌اند):
- یک endpoint عمومی ثبت «کار تحلیل متن» مثل `POST /api/ai/jobs` با قرارداد `AiCoreSubmitJobRequest`/`AiCoreSubmitJobResponse`.
- نوع کار جدید `TextAnalysis` + پردازش آن در Worker (الان Worker فقط `EmbeddingBatch` را اجرا می‌کند).
- ارسال callback امضاشده (HMAC) به `‎/api/ai/callbacks/aicore` در پایان/خطا، با `correlationId` و `callbackUrl`.

**پیش‌فرض حالت یکپارچه‌سازی:** در `appsettings`, `AI:AiCore:Mode` هنوز `AsyncAiCoreJob` است؛ تا قبل از تکمیل AiCore باید موقتاً `SyncAiCoreDirect` باشد تا تحلیل end-to-end کار کند (طبق گام ۱ سند discovery). فقط پس از AI-11 به async برگردد.

---

## ۲. فازهای باقی‌مانده‌ی AI

**AI-10 — Fallback ورکر محلی Ollama:** مقدار `LocalOllamaDirect` در enum هست اما dispatcher ورکر برای آن «not supported» پرتاب می‌کند. باید مسیر اجرای کامل تحلیل به‌صورت محلی با Ollama (بدون AiCore) پیاده شود تا در نبود AiCore هم سامانه تحلیل بدهد.

**AI-12 — سخت‌سازی تولید:**
- احراز هویت callback: اگر نه `ApiKey` و نه `CallbackSecret` تنظیم نشده باشد، فعلاً عبور می‌کند؛ در تولید باید حداقل یکی الزامی شود.
- SignalR در محیط چنداینستنسی به backplane (مثل Redis) نیاز دارد؛ الان فقط تک‌اینستنس درست کار می‌کند.
- محدودیت نرخ (rate limiting) روی endpoint callback، چرخش اسرار، و CORS/HTTPS هاب.
- مشاهده‌پذیری (logging/metrics) برای کارهای AI.

---

## ۳. عمق و کیفیت تحلیل AI (محتوا، نه فقط زیرساخت)
- `ProcurementRuleEvaluator` فقط چند قاعده‌ی نمونه دارد؛ نیاز به مجموعه‌ی واقعی قوانین خرید/مناقصه.
- RAG / جستجوی هوشمند اسناد (فاز ۶ چشم‌انداز) پیاده نشده؛ AiCore فقط `embeddings` و `similarity` خام دارد، خط لوله‌ی بازیابی وجود ندارد.
- **باگ timeout در `AiCoreClient`** (سمت sync): یک `CancellationTokenSource` با timeout ساخته می‌شود اما به `http.SendAsync` مقدار `CancellationToken.None` پاس داده شده؛ پس timeout هرگز اعمال نمی‌شود. باید توکن لینک‌شده پاس داده شود.

---

## ۴. Placeholderهای زیرساخت
- `PetroProcure.Worker/Worker.cs` — پاکسازی فایل‌های یتیم فقط placeholder است.
- `Infrastructure/Storage/FileScanning.cs` — اسکن ضدبدافزار/تطبیق فایل فقط placeholder است.

## ۵. درستی داده
- `FakeDirectory` با GUIDهای ثابت هنوز در `PurchaseFileDetail` استفاده می‌شود (نگاشت واحد در افزودن قلم، نام واحدها)؛ باید از `LookupCacheService`/API بیاید تا با داده‌ی واقعی ناسازگار نشود.

---

## ۶. UI/UX باقی‌مانده
- اتصال کامپوننت async `AiJobPanel` به صفحات جزئیات مناقصه/قرارداد/سفارش خرید/رسید انبار (endpointهای analyze آن‌ها از قبل موجود است) — فعلاً فقط در پرونده خرید است.
- نمای کارتی واقعی جداول در موبایل (الان فقط اسکرول صیقلی است).
- ممیزی دسترس‌پذیری: `aria-label` برای آیکن-دکمه‌ها، کنتراست متن کم‌رنگ، فوکوس.
- `README.md` کهنه است (ماژول‌های پیاده‌شده را «پیاده‌نشده» معرفی می‌کند) — به‌روزرسانی شود.

---

## ۷. ساخت و تست (اقدام لازم)
- محیط ساندباکس به‌دلیل کمبود فضای دیسک بالا نیامد؛ روی ماشین محلی `dotnet build` و `dotnet test` اجرا شود.
- تست‌های یکپارچه (Callback، SignalR، Authorization) به SQL Server محلی نیاز دارند.
- تست‌های SignalR از long-polling روی TestServer استفاده می‌کنند؛ پایداری آن‌ها محلی تأیید شود.
- پیشنهاد: یک تست end-to-end برای حالت `SyncAiCoreDirect` (ایجاد کار → پردازش ورکر → نتیجه) که AiCore را Mock کند.

---

## خلاصه‌ی اولویت
1. تکمیل AiCore (AI-11) + برگرداندن پیش‌فرض به Sync تا آن زمان — تا تحلیل واقعاً end-to-end شود.
2. AI-10 (Ollama محلی) برای استقلال از AiCore.
3. رفع باگ timeout و حذف `FakeDirectory`.
4. عمق‌بخشی به قوانین AI و RAG.
5. AI-12 سخت‌سازی تولید + گسترش UI تحلیل به سایر ماژول‌ها.
