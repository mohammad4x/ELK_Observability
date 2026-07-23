<div dir="rtl">

# مشاهده‌پذیری: یک سامانه، یک تحقیق

[English](README.md)

این مخزن همراه یک ارائهٔ ۳۰ دقیقه‌ای و backend-focused دربارهٔ مشاهده‌پذیری است. ابتدا مفاهیم پایه را توضیح می‌دهد و سپس در یک سامانهٔ نهایی .NET، لاگ، metric، trace، OpenTelemetry، RabbitMQ و Elastic را کنار هم نشان می‌دهد.

این مخزن دیگر ساختار دوره‌ای چندمرحله‌ای ندارد. فقط یک solution را باز کنید:

```text
ELKStack.slnx
```

## جریان ارائه

| زمان | موضوع | یادداشت گوینده |
| --- | --- | --- |
| ۰ تا ۳ دقیقه | مسئلهٔ اصلی | [چرا مشاهده‌پذیری؟](talk/README.fa.md#۱-مسئلهٔ-اصلی) |
| ۳ تا ۸ | logs، metrics و traces | [شواهد متفاوت](talk/README.fa.md#۲-logs-metrics-و-traces) |
| ۸ تا ۱۱ | structured logs و Serilog | [لاگ قابل جست‌وجو](talk/README.fa.md#۳-structured-logging-و-serilog) |
| ۱۱ تا ۱۷ | OpenTelemetry | [vendor-neutral](talk/README.fa.md#۴-opentelemetry) |
| ۱۷ تا ۲۰ | agent در برابر کد | [مقایسه](talk/README.fa.md#۵-agent-در-برابر-instrumentation-کدنویسیشده) |
| ۲۰ تا ۲۲ | Java، Python و ASP.NET | [اکوسیستم‌ها](talk/README.fa.md#۶-اکوسیستمها) |
| ۲۲ تا ۲۴ | ابزارها | [چشم‌انداز ابزار](talk/README.fa.md#۷-ابزارها) |
| ۲۴ تا ۲۶ | چرا Elastic | [دلیل انتخاب](talk/README.fa.md#۸-چرا-elastic-در-این-دمو) |
| ۲۶ تا ۳۰ | دمو | [اسکریپت دمو](talk/README.fa.md#۹-دمو) |

## سامانهٔ دمو

```text
POST /api/bookings
  → Booking Service → RabbitMQ → Payment Service
  → RabbitMQ → Booking Service → RabbitMQ → Notification Service
```

| پروژه | نقش |
| --- | --- |
| [BookingService](src/ELKStack.BookingService/Program.cs) | دریافت رزرو و نگهداری وضعیت آن |
| [PaymentService](src/ELKStack.PaymentService/Program.cs) | درخواست، تکمیل یا شکست پرداخت |
| [NotificationService](src/ELKStack.NotificationService/Program.cs) | ارسال اعلان |
| [Contracts](src/ELKStack.Contracts/IntegrationEvents.cs) | رویدادها و EventId/CorrelationId/CausationId |
| [Observability](src/ELKStack.Observability/ObservabilityExtensions.cs) | correlation برای HTTP و MassTransit |
| [Service Defaults](Aspire/ELKStack.ServiceDefaults/Extensions.cs) | Serilog، OpenTelemetry و health check |
| [AppHost](Aspire/ELKStack.AppHost/AppHost.cs) | RabbitMQ، Elastic، Kibana، Collector و سرویس‌ها |

## اجرای دمو

```powershell
dotnet run --project Aspire/ELKStack.AppHost/ELKStack.AppHost.csproj
```

یک booking با `scenario` برابر `PaymentFailure` ارسال کنید و از symptom به structured log، سپس trace و در نهایت CorrelationId/CausationId برسید.

![نمای Kibana](assets/example.png)

**نتیجهٔ اصلی:** metric نشانه را می‌یابد، trace مسیر را نشان می‌دهد، log تصمیم محلی را توضیح می‌دهد، و context کسب‌وکاری همه را به یک workflow وصل می‌کند.

</div>
