<div dir="rtl">

# ۰۴ — لاگ، گراف اجرا نیست

[English](README.md)

ده‌ها لاگ correlated هنوز یک فهرست تخت‌اند؛ parent/child، parallelism و زمان صرف‌شده را نشان نمی‌دهند. برای این پرسش از trace توزیع‌شده استفاده می‌کنیم.

```text
ActivitySource  Activity می‌سازد
Activity.Current  context جاری را حمل می‌کند
ActivityListener  Activityها را مشاهده می‌کند
OpenTelemetry SDK  آن‌ها را sample، enrich و export می‌کند
Activity ≈ Span
```

ASP.NET Core و `HttpClient` به‌طور خودکار Activity می‌سازند و MassTransit از source با نام `MassTransit` استفاده می‌کند. trace مربوط به `POST /api/bookings` را در Kibana باز کنید و spanهای HTTP و publish/consume را دنبال کنید.

trace عملیات شناخته‌شده را توضیح می‌دهد؛ هنوز نمی‌دانیم بدون دانستن TraceId چگونه نشانهٔ مشکل را پیدا کنیم.

</div>
