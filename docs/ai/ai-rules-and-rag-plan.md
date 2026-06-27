# طرح اجرایی مرحله‌ای: عمق‌بخشی به قوانین AI و RAG

این سند یک نقشه‌ی راه عملی و مرحله‌ای است، بر پایه‌ی کد موجود پروژه (نه طراحی از صفر). دو مسیر موازی دارد:
- **مسیر A:** عمیق‌سازی موتور قوانین خرید/حقوقی (Procurement Rules).
- **مسیر B:** RAG (جستجوی هوشمند اسناد + تحلیل مستند به منبع).

## آنچه از قبل موجود است (پایه‌ها)
- مدل قوانین نسخه‌دار: `ProcurementRule` + `ProcurementRuleVersion` با `RuleType`, `RuleSeverity`, `RuleEvaluationMode`, `ConditionType`, `ConditionValue`, و اتصال به `LegalArticleId`/`LegalClauseId`/`LegalReference`. گردش وضعیت Draft→PendingApproval→Active.
- موتور قطعی فعلی: `DeterministicProcurementRuleEvaluator.EvaluateAsync` با یک switch کوچک شامل فقط ۶ شرط (`alwayspass`, `alwaysfail`, `requireddocumenttype`, `minimumitems`, `hastender`, `purchasefilestatus`).
- `RuleEvaluationContext` فقط `DocumentTypes`, `ItemCount`, `HasTender`, `Status` دارد.
- تحلیل مبتنی بر AiCore: `AnalyzeLegalComplianceAsync` + `IAiLegalRuleContextBuilder` + ذخیره‌ی `ProcurementRuleEvaluation`/`ProcurementRuleFinding`.
- بلوک‌های RAG: AiCore دارای `POST /api/ai/embeddings` و `CalculateSimilarityAsync`؛ بندهای حقوقی (`LegalClause`) از قبل «تکه‌شده» با `Body`/`SearchText`/`AppliesTo`/`Tags`.
- صف کار AI + Worker (از فازهای AI-03..AI-08) قابل استفاده برای پردازش پس‌زمینه‌ی ingestion.

---

# مسیر A — عمیق‌سازی موتور قوانین

## A1) غنی‌سازی Context ارزیابی
- گسترش `RuleEvaluationContext` و `IRuleEvaluationContextBuilder` تا شامل: مبلغ برآوردی/نهایی + ارز، تعداد/جمع اقلام، تاریخ‌ها و مهلت‌ها (ایجاد، مهلت استعلام/مناقصه)، نوع مناقصه، تعداد تأمین‌کننده/پیشنهاد، وضعیت تأییدها، انواع اسناد موجود، واحد درخواست‌کننده.
- خروجی: یک DTO کانتکست که همه‌ی `RuleType`ها (Threshold/Deadline/Workflow/Document/Evaluation) را پوشش دهد.
- پذیرش: تست واحد که برای یک پرونده‌ی نمونه همه‌ی فیلدها درست پر شوند.

## A2) موتور شرط داده‌محور (مهم‌ترین گام)
- تعریف یک «اسکیمای شرط» در `ConditionValue` به‌صورت JSON تا قوانین بدون تغییر کد اضافه شوند. نمونه:
  ```json
  { "op": ">=", "field": "FinalAmount", "value": 1000000000, "currency": "IRR" }
  { "op": "exists", "field": "DocumentTypes", "value": "TechnicalSpecification" }
  { "op": "before", "field": "TenderDeadline", "value": "P7D" }   // ۷ روز قبل
  { "op": "count>=", "field": "SupplierCount", "value": 3 }
  ```
- پیاده‌سازی `IConditionEvaluator` با خانواده‌ی عملگرها به‌تفکیک `RuleType`:
  - Threshold: `>= , <= , > , < , between` روی مبالغ/تعداد.
  - Deadline: `before/after/within` روی تاریخ‌ها (با بازه‌ی ISO-8601).
  - Document: `exists/missing/anyOf/allOf` روی انواع اسناد.
  - Checklist/Workflow: وضعیت‌ها و تأییدها.
  - Exception: شرط معافیت که نتیجه را override می‌کند.
- جایگزینی switch فعلی با این موتور؛ حفظ سازگاری با ۶ شرط قبلی.
- پذیرش: حداقل برای هر `RuleType` یک شرط واقعی + مجموعه تست واحد جدول‌محور (Pass/Fail/Warning/NotApplicable).

## A3) کتابخانه‌ی قوانین واقعی (Seed)
- وارد کردن مجموعه‌قوانین واقعی خرید/مناقصه (آیین‌نامه‌های سازمانی/NIOC) به‌صورت seed: یک `ProcurementRuleSet` + نسخه‌های `ProcurementRuleVersion` فعال، هرکدام متصل به ماده/بند حقوقی (`LegalArticle`/`LegalClause`) و `LegalReference`.
- ابزار import (CSV/JSON) برای نگه‌داری قوانین توسط واحد حقوقی.
- پذیرش: N قاعده‌ی فعال با ارجاع حقوقی؛ صفحات Admin (`ProcurementRulesPage`/`Edit`/`Test`) آن‌ها را نشان دهند و چرخه‌ی تأیید کار کند.

## A4) ارزیابی ترکیبی (Deterministic + AI)
- برای `EvaluationMode = SemiAutomatic/ManualReview` و `RuleType = Evaluation`، مسیردهی به AiCore (`AnalyzeLegalComplianceAsync`) با کانتکست قاعده + متن بند حقوقی.
- ادغام یافته‌های قطعی و AI در `ProcurementRuleEvaluation`؛ یافته‌های AI همیشه «پیشنهادی» و با `RuleResult.NeedHumanReview` علامت‌گذاری شوند.
- پذیرش: یک قاعده‌ی SemiAutomatic یافته‌ای با توضیح و استناد حقوقی تولید کند؛ قوانین قطعی بدون تغییر بمانند.

## A5) اتصال یافته‌ها به گردش‌کار (Gating)
- یافته‌های `RuleSeverity.Blocking` + `RuleResult.Fail` مانع گذارهای حساس (مثل انتشار مناقصه/صدور سفارش) شوند، با امکان override توسط کاربر مجاز و ثبت در `LegalRuleAuditLog`.
- پذیرش: گذار مسدود می‌شود مگر override مجاز (ثبت‌شده).

## A6) UI و گزارش
- پنل نتیجه‌ی ارزیابی (Pass/Fail/Warning به‌تفکیک قاعده + استناد حقوقی + راهکار رفع) در پرونده خرید/مناقصه.
- گزارش PDF انطباق حقوقی.
- پذیرش: ارزیابی قابل‌مشاهده و قابل‌خروجی.

---

# مسیر B — RAG (جستجوی هوشمند اسناد)

## B1) Corpus و Chunking
- تعریف پیکره: بندهای حقوقی (از قبل تکه‌شده) + اسناد پرونده‌ها (نیازمند استخراج متن: PDF/DOCX → متن → تکه‌های ~۵۰۰–۱۰۰۰ توکن با هم‌پوشانی).
- موجودیت جدید `AiDocumentChunk` (SourceType, SourceId, Ordinal, Text, TokenCount, ContentHash, Metadata).
- سرویس `IChunkingService` + استخراج متن (برای PDF از زیرساخت موجود؛ برای فرمت‌های دیگر افزودنی).
- پذیرش: chunkها برای بندهای حقوقی و یک سند آپلودی تولید شوند.

## B2) Embedding Store + Index
- جدول `AiEmbedding` (ChunkId, Model, Vector به‌صورت varbinary/JSON, CreatedAt).
- انتزاع `IEmbeddingIndex` با `UpsertAsync(chunkId, vector)` و `SearchAsync(queryVector, topK, filter)`.
- پیاده‌سازی فاز ۱: cosine به‌صورت brute-force در app برای پیکره‌ی متوسط؛ پشت همان interface بعداً به VECTOR در SQL Server 2025 یا یک vector DB (مثل Qdrant) سوییچ شود.
- تولید embedding از AiCore (`/api/ai/embeddings`، به‌صورت batch).
- پذیرش: embeddingها تولید و ذخیره شوند؛ جستجو top-K را با cosine برگرداند.

## B3) خط لوله‌ی Ingestion (پس‌زمینه)
- نوع کار جدید در صف AI: `EmbeddingIngestion`؛ هنگام آپلود سند یا انتشار سند حقوقی، chunk + embed + index انجام شود (idempotent بر اساس `ContentHash`).
- پذیرش: آپلود سند، ingestion را ماشه می‌کند؛ آپلود مجدد embeddingها را به‌روزرسانی می‌کند.

## B4) API بازیابی (Retrieval)
- `IRagRetriever.RetrieveAsync(query, scope, topK)` → embedding پرسش (AiCore) → جستجوی index → بازگرداندن تکه‌های رتبه‌بندی‌شده با ارجاع منبع.
- Scopeها: همین پرونده، پیکره‌ی حقوقی، مناقصه — با اعمال محدوده‌ی دسترسی کاربر.
- پذیرش: پرسش، تکه‌های مرتبط با لینک منبع برگرداند.

## B5) تحلیل مستند به منبع (Grounded)
- تزریق top-K تکه‌ی بازیابی‌شده (با citation) به کانتکست تحلیل AiCore تا پاسخ‌ها مستند و دارای ارجاع باشند.
- انواع تحلیل جدید: `AskAboutFile` (پرسش آزاد)، `FindRelevantRegulations`.
- فقط متن تکه + citation ارسال شود (بدون داده‌ی حساس/خام).
- پذیرش: پاسخ AI شامل ارجاع به بند/بخش سند باشد؛ کاهش محسوس hallucination.

## B6) UI
- پنل «جستجوی هوشمند اسناد» + جعبه‌ی پرسش‌وپاسخ در پرونده خرید/صفحات حقوقی؛ نمایش پاسخ + منابع قابل‌کلیک.
- پذیرش: کاربر پرسش می‌کند، پاسخ مستند + منابع می‌گیرد.

---

# موارد مشترک (Cross-cutting)
- **امنیت:** ingestion/retrieval فقط در محدوده‌ی مجاز کاربر؛ جلوگیری از نشت بین پرونده‌ها.
- **هزینه:** embedding دسته‌ای، cache، و عدم re-embed بدون تغییر hash.
- **ارزیابی کیفیت:** مجموعه‌ی طلایی پرسش‌وپاسخ + سنجش precision@k برای retrieval و صحت یافته‌ها برای قوانین.
- **نسخه‌بندی مدل:** پین‌کردن مدل embedding؛ در صورت تغییر مدل، re-index کامل.

# توالی و وابستگی‌ها
- مسیر A مستقل است: A1→A2→A3 بدون نیاز به AiCore. A4 به AiCore (حالت sync که الان کار می‌کند) وابسته است.
- مسیر B به `embeddings` در AiCore وابسته است که هم‌اکنون به‌صورت sync موجود است؛ پس B1→B4 همین حالا قابل اجراست.
- پیشنهاد توالی کلی:
  1. موازی: A1→A2→A3 (عمق قطعی) و B1→B2→B3→B4 (بازیابی RAG).
  2. هم‌گرایی: A4 + B5 (مستندسازی AI).
  3. UI (A6/B6) + سخت‌سازی و ارزیابی کیفیت.

# تخمین تلاش (نسبی)
- A1 کوچک، A2 متوسط‑بزرگ (هسته)، A3 متوسط (محتوای حقوقی)، A4 کوچک‑متوسط، A5 کوچک، A6 متوسط.
- B1 متوسط (استخراج متن)، B2 متوسط، B3 کوچک‑متوسط، B4 کوچک، B5 کوچک‑متوسط، B6 متوسط.

# اولین گام پیشنهادی برای شروع (Quick win)
1. A2: موتور شرط داده‌محور + تست‌های جدول‌محور (بیشترین ارزش، بدون وابستگی خارجی).
2. به‌موازات B2: انتزاع `IEmbeddingIndex` + پیاده‌سازی cosine ساده روی بندهای حقوقی موجود (که از قبل تکه‌اند) تا RAG اولیه روی پیکره‌ی حقوقی به‌سرعت کار کند.
