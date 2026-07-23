<div dir="rtl">

# ۰۳ — دنبال کردن یک عملیات کسب‌وکاری

[English](README.md)

یک رزرو از HTTP، RabbitMQ و سه سرویس عبور می‌کند. حتی اگر هر رکورد `BookingId` داشته باشد، این شناسه هویت workflow نیست؛ پیام بعدی ممکن است فقط `PaymentId` یا `NotificationId` داشته باشد.

راه‌حل، انتقال metadata کسب‌وکاری در هر integration event است:

```text
CorrelationId  عضویت در یک workflow
EventId        هویت یک نمونهٔ پیام
CausationId    پیام قبلی که علت این پیام است
```

همهٔ رکوردهای workflow یک `CorrelationId` دارند و هر پیام فرزند، `CausationId` را برابر `EventId` پیام والد قرار می‌دهد.

کدهای مهم: `CorrelationMiddleware` برای HTTP، `CorrelationPublishFilter` برای نوشتن headerهای MassTransit، و `CorrelationConsumeFilter` برای بازگرداندن context در consumer هستند.

یک CorrelationId را در هر سه سرویس جست‌وجو کنید و زنجیره را با EventId/CausationId بسازید. اما به یاد داشته باشید: correlation عضویت می‌دهد، نه ساختار و زمان اجرا.

</div>
