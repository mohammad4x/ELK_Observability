<div dir="rtl">

# مشاهده‌پذیری: فهم یک سامانهٔ توزیع‌شده

[English](README.md)

این راهنما مفاهیم را با workflow رزرو این مخزن توضیح می‌دهد، اما برای .NET، Java/Spring، Python/Django و دیگر سامانه‌های توزیع‌شده کاربرد دارد.

## logs، metrics و traces

| سیگنال | چه چیزی ثبت می‌کند؟ | پرسش مناسب | محدودیت به‌تنهایی |
| --- | --- | --- | --- |
| Log | رخداد، تصمیم یا exception با جزئیات | کد چه گزارش کرد؟ | گراف اجرا نیست |
| Metric | اندازه‌گیری تجمیعی از رخدادهای زیاد | آیا کل سامانه تغییر کرده؟ | علت یک درخواست را نمی‌گوید |
| Trace | گراف زمان‌دار یک عملیات | کدام hop کند/ناموفق بود؟ | به symptom یا trace نیاز دارد |

metric برای یافتن تغییر کلی مناسب است، trace مسیر و زمان یک عملیات را برمی‌گرداند، و log جزئیات محلی مانند exception یا پاسخ provider را اضافه می‌کند. `CorrelationId` می‌گوید چه eventهایی یک workflow هستند و `CausationId` علت event بعدی را نشان می‌دهد.

## structured logs و Serilog

```csharp
logger.LogWarning(
    "Payment {PaymentId} failed for booking {BookingId}: {Reason}",
    paymentId, bookingId, reason);
```

در این مدل شناسه‌ها propertyهای نام‌دار و قابل جست‌وجو هستند. `ILogger<T>` abstraction استاندارد .NET است و `ILoggerFactory` رکوردها را به providerها می‌فرستد. ASP.NET Core این pipeline را با `WebApplication.CreateBuilder` و DI آماده می‌کند. Serilog در این مخزن provider ساخت‌یافته، enrichment، sink، خروجی console و request logging را فراهم می‌کند؛ کد دامنه همچنان به `ILogger<T>` وابسته است.

## OpenTelemetry

OpenTelemetry یک استاندارد و ecosystem شامل API، SDK، instrumentation، semantic convention، Collector و OTLP است؛ database یا dashboard نیست.

```text
instrumentation → OTel SDK → OTLP → Collector → backend منتخب
```

این مرز باعث می‌شود instrumentation برنامه vendor-neutral باقی بماند. جابه‌جایی backend همچنان dashboard، query، alert و retention را تغییر می‌دهد، اما spanهای دستی برنامه را مجبور به بازنویسی نمی‌کند. در .NET این مدل بر پایهٔ `Activity`، `ActivitySource` و `Meter` است.

## agent در برابر instrumentation کدنویسی‌شده

auto-instrumentation برای مرزهای HTTP، database، messaging و runtime با تغییر کم در کد سریع است. بسته به runtime می‌تواند Java agent، monkey patch در Python، startup hook/profiling در .NET یا eBPF باشد. instrumentation داخل کد برای intent کسب‌وکاری—مثلاً زمان provider پرداخت یا شمارش نتیجهٔ دامنه—است.

بهترین روش ترکیب هر دو است: خودکار برای مرزهای فریم‌ورک و کد برای پرسش‌های کسب‌وکاری.

## اکوسیستم‌ها

ASP.NET Core از `ActivitySource`/`Meter`، Java/Spring از Java agent و OTel Java API، و Python/Django از `opentelemetry-instrument` و OTel Python API استفاده می‌کنند. context propagation، resource attribute، semantic convention و OTLP مفاهیم مشترک‌اند.

## ابزارها و Elastic

برای metrics ابزارهای شناخته‌شده Prometheus، Grafana Mimir و VictoriaMetrics هستند؛ برای logs، Elastic، Loki و OpenSearch؛ و برای traces، Jaeger، Tempo، Zipkin و Elastic APM.

Elastic در این دمو انتخاب شده چون logs، metrics، traces و شناسه‌های workflow را در یک سطح تحقیقاتی جست‌وجوپذیر کنار هم قرار می‌دهد؛ در عین حال application contract، OTLP/OpenTelemetry باقی می‌ماند. هزینه، licensing و queryهای اختصاصی backend همچنان trade-off واقعی‌اند.

## خواندن workflow

```text
TraceId       گراف و زمان اجرای فنی
CorrelationId عضویت در workflow کسب‌وکاری
EventId       هویت یک event/message
CausationId   event قبلی که علت event فعلی است
```

این تفکیک برای retry، پیام زمان‌بندی‌شده، delayed work و فرآیندهای طولانی مهم است؛ یک workflow می‌تواند از یک trace بیشتر عمر کند.

</div>
