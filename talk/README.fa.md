<div dir="rtl">

# یادداشت ارائهٔ ۳۰ دقیقه‌ای مشاهده‌پذیری

[English](README.md)

## ۱. مسئلهٔ اصلی

با این جمله شروع کنید: «کاربر تلاش کرد رزرو بسازد؛ مشکلی پیش آمد. چه شد؟» پایش به پرسش‌های از پیش شناخته‌شده مثل availability، نرخ خطا و latency پاسخ می‌دهد. مشاهده‌پذیری برای پرسش ناشناخته است: چرا این عملیات شکست خورد و چه مسیری را طی کرد؟

## ۲. logs، metrics و traces

| سیگنال | دلیل وجود | پرسش مناسب | محدودیت |
| --- | --- | --- | --- |
| Log | جزئیات رخداد، تصمیم و exception | کد چه گفت؟ | گراف اجرا نیست |
| Metric | اندازه‌گیری تجمیعی ارزان | آیا کل سامانه تغییر کرده؟ | علت یک درخواست نیست |
| Trace | گراف زمان‌دار parent/child | کدام hop کند/ناموفق بود؟ | به نقطهٔ شروع نیاز دارد |

جملهٔ کلیدی: **metric دود را پیدا می‌کند؛ trace مسیر را نشان می‌دهد؛ log تصمیم محلی را توضیح می‌دهد.** `CorrelationId` عضویت در workflow و `CausationId` علت پیام بعدی را می‌دهد.

## ۳. structured logging و Serilog

```csharp
logger.LogWarning(
    "Payment {PaymentId} failed for booking {BookingId}: {Reason}",
    paymentId, bookingId, reason);
```

در این مدل شناسه‌ها propertyهای نام‌دار و قابل جست‌وجو هستند، نه بخشی از متن. در .NET کد دامنه از `ILogger<T>` استفاده می‌کند و provider انتخاب‌شده رکوردها را ارسال می‌کند. ASP.NET Core logging و DI را از `WebApplication.CreateBuilder` آماده می‌کند. Serilog در این پروژه provider ساخت‌یافته، enrichment، sink و request logging است.

## ۴. OpenTelemetry

OpenTelemetry استاندارد و مجموعه‌ای از API، SDK، instrumentation، Collector، semantic conventions و OTLP است؛ backend ذخیره‌سازی نیست.

```text
instrumentation → OTel SDK → OTLP → Collector → backend
```

این مرز vendor-neutral است: instrumentation برنامه ثابت می‌ماند، هرچند dashboard، query، alert و retention در backendها متفاوت‌اند. در .NET، OTel بر پایهٔ `Activity`، `ActivitySource` و `Meter` است.

## ۵. agent در برابر instrumentation کدنویسی‌شده

agent/auto-instrumentation سریع برای HTTP، database، messaging و سرویس‌های legacy است، اما معمولاً intent کسب‌وکاری را نمی‌فهمد. instrumentation داخل کد برای provider payment، خطاهای دامنه و metric کسب‌وکاری مناسب است. هر دو را استفاده کنید: خودکار برای مرزهای فریم‌ورک و کد برای پرسش‌های کسب‌وکاری.

## ۶. اکوسیستم‌ها

ASP.NET Core از `ActivitySource` و `Meter`، Java/Spring از Java agent و OTel Java API، و Python/Django از `opentelemetry-instrument` و OTel Python API استفاده می‌کنند. مفهوم مشترک context propagation، resource attribute، semantic convention و OTLP است.

## ۷. ابزارها

برای metricها: Prometheus، Grafana Mimir، VictoriaMetrics. برای logها: Elastic، Loki، OpenSearch. برای traceها: Jaeger، Tempo، Zipkin و Elastic APM. این‌ها هم‌پوشانی دارند؛ جدول تصمیم خرید نیست.

## ۸. چرا Elastic در این دمو؟

یک محیط تحقیقاتی برای جست‌وجوی log، trace، metric و شناسه‌های کسب‌وکاری؛ جست‌وجوی طبیعی `CorrelationId`/`EventId`/`CausationId`؛ پذیرش OTLP بدون قفل کردن instrumentation برنامه؛ تجربهٔ APM مناسب دمو؛ و Collector به‌عنوان نقطهٔ مستقل batching/sampling/routing. صادقانه بگویید که هزینه، licensing و queryهای اختصاصی backend هم وجود دارند.

## ۹. دمو

`ELKStack.slnx` را باز کنید، AppHost را اجرا کنید، یک رزرو Normal و سپس PaymentFailure بفرستید. از symptom شروع کنید، فیلدهای log را نشان دهید، trace HTTP تا RabbitMQ و Payment را باز کنید، سپس CorrelationId و زنجیرهٔ EventId/CausationId را دنبال کنید. با مسیر metric → trace → log → workflow تمام کنید.

</div>
