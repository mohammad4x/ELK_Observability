<div dir="rtl">

# ۰۷ — تحقیق کامل: اجرا و علیت کسب‌وکاری

[English](README.md)

هدف این است که برای یک رزرو شکست‌خورده، هم علت فنی و هم workflow کسب‌وکاری متاثر را نشان دهیم.

```text
TraceId       گراف و timeline اجرای فنی
CorrelationId عضویت در workflow کسب‌وکاری
EventId       هویت یک پیام
CausationId   پیام قبلیِ علت
```

این شناسه‌ها یکی نیستند. retry، delayed message، scheduled work یا تأیید انسانی می‌تواند همان workflow را بعد از پایان طبیعی trace اولیه ادامه دهد.

```text
metric: خطاهای پرداخت بالا می‌روند
  → traceهای پرداخت ناموفق را فیلتر کن
    → span provider و error/duration را ببین
      → propertyهای لاگ را بخوان
        → CorrelationId و CausationId را در workflow دنبال کن
```

`Normal` و `PaymentFailure` اکنون پیاده‌سازی شده‌اند. سناریوهای بعدی این مرحله `SlowPayment`، `NotificationFailure`، `MissingTracePropagation` و `PaymentRetryThenSuccess` هستند.

</div>
