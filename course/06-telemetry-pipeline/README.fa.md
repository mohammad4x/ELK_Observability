<div dir="rtl">

# ۰۶ — خط لولهٔ telemetry

[English](README.md)

این مرحله مسئولیت هر بخش را روشن می‌کند:

```text
Instrumentation برنامه
  → OpenTelemetry SDK
  → OTLP
  → OpenTelemetry Collector
  → Elastic APM Server
  → Elasticsearch
  → Kibana
```

| جزء | مسئولیت |
| --- | --- |
| Instrumentation | تولید telemetry فریم‌ورک یا سفارشی |
| OTel SDK | resource، listener، sampling، processor و exporter |
| OTLP | پروتکل vendor-neutral انتقال telemetry |
| Collector | دریافت، batch، پردازش و route کردن داده |
| APM Server | endpoint ورودی Elastic APM |
| Elasticsearch | ذخیره و index کردن |
| Kibana | جست‌وجو، visualization و تحقیق |

instrumentation خودکار مرزهای فریم‌ورک را می‌بیند؛ instrumentation دستی عملیات معنادار دامنه، مانند فراخوانی payment provider، را اضافه می‌کند. OpenTelemetry ابزار خط لوله است، نه تعریف مشاهده‌پذیری.

</div>
