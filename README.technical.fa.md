<div dir="rtl">

# راهنمای فنی: مشاهده‌پذیری در یک سامانهٔ توزیع‌شدهٔ .NET

[English](README.technical.md)

این مخزن راهنمایی عملی برای فهم مشاهده‌پذیری در یک سامانهٔ event-driven است. ابتدا دلیل وجود logs، metrics، traces، structured logging، OpenTelemetry و Elastic را توضیح می‌دهد و سپس همهٔ آن‌ها را در یک workflow رزرو نشان می‌دهد.

[ELKStack.slnx](ELKStack.slnx) را برای سامانهٔ کامل باز کنید و [راهنمای مشاهده‌پذیری](README.fa.md) را برای مفاهیم بخوانید.

## پرسش اصلی

> «مشتری تلاش کرد رزرو بسازد؛ مشکلی پیش آمد. چه شد؟»

در معماری توزیع‌شده یک درخواست از HTTP، سرویس‌ها، صف‌ها و workerهای پس‌زمینه عبور می‌کند. مشاهده‌پذیری یعنی بتوانیم از نشانهٔ کلی به شواهد یک عملیات برسیم.

```text
Metric: خطاها زیاد شده‌اند
  → Trace: مسیر کند یا ناموفق
    → Log: تصمیم و علت محلی
      → Context: workflow رزرو متاثر
```

## محتوای راهنما

| موضوع | دلیل اهمیت |
| --- | --- |
| [logs، metrics و traces](talk/README.fa.md#۲-logs-metrics-و-traces) | هر سیگنال یک پرسش متفاوت را پاسخ می‌دهد. |
| [structured logs و Serilog](talk/README.fa.md#۳-structured-logging-و-serilog) | فیلد قابل جست‌وجو از متن قابل‌خواندن قوی‌تر است. |
| [OpenTelemetry](talk/README.fa.md#۴-opentelemetry) | مرز vendor-neutral برای instrumentation و انتقال telemetry. |
| [agent و instrumentation کد](talk/README.fa.md#۵-agent-در-برابر-instrumentation-کدنویسیشده) | انتخاب سطح درست کنترل. |
| [ابزارها و Elastic](talk/README.fa.md#۷-ابزارها) | جای Prometheus، Tempo، Jaeger، Loki و Elastic. |

## سامانهٔ نمونه

```text
POST /api/bookings
  → Booking Service → RabbitMQ → Payment Service
  → RabbitMQ → Booking Service → RabbitMQ → Notification Service
```

| پروژه | مسئولیت |
| --- | --- |
| [BookingService](src/ELKStack.BookingService/Program.cs) | دریافت رزرو و نگهداری وضعیت |
| [PaymentService](src/ELKStack.PaymentService/Program.cs) | پردازش نتیجهٔ پرداخت |
| [NotificationService](src/ELKStack.NotificationService/Program.cs) | واکنش به رزرو تاییدشده |
| [Contracts](src/ELKStack.Contracts/IntegrationEvents.cs) | eventها و شناسه‌های کسب‌وکاری |
| [Observability](src/ELKStack.Observability/ObservabilityExtensions.cs) | correlation و causation برای HTTP و MassTransit |
| [AppHost](Aspire/ELKStack.AppHost/AppHost.cs) | اجرای RabbitMQ، Collector، Elastic و سرویس‌ها |

## اجرا

```powershell
dotnet run --project Aspire/ELKStack.AppHost/ELKStack.AppHost.csproj
```

یک درخواست با `scenario` برابر `PaymentFailure` بفرستید؛ سپس از symptom به فیلدهای ساخت‌یافتهٔ log، trace و در نهایت `CorrelationId`/`CausationId` برسید.

![نمای Kibana](assets/example.png)

</div>
