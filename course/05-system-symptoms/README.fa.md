<div dir="rtl">

# ۰۵ — نشانه‌های سامانه

[English](README.md)

اگر مشتری TraceId نداده باشد، برای کشف وضعیت بدشونده به metric نیاز داریم، نه مرور traceهای تک‌به‌تک.

```text
request rate       آیا کار وارد سامانه می‌شود؟
failure count      آیا موفق است؟
duration histogram آیا کند شده است؟
queue depth        آیا کار عقب مانده است؟
```

metricها view تجمیعی و مناسب alerting هستند؛ traceها view با وضوح بالا برای یک عملیات‌اند. در وضعیت فعلی metricهای HTTP/runtime موجودند. سناریوی `SlowPayment` و histogram مدت پرداخت در گام بعدی اضافه می‌شود تا مسیر تحقیق metric → trace → log کامل شود.

</div>
