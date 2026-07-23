<div dir="rtl">

# ۰۱ — سامانهٔ مبهم: یک چیز شکست خورد؛ کجا؟

[English](README.md)

کاربر از شکست رزرو خبر می‌دهد و خروجی فقط چنین است:

```text
Booking requested
Processing payment
Payment failed
Booking requested
Processing payment
Payment completed
```

کدام رزرو شکست خورد؟ علت چه بود؟ آیا مشکل در HTTP، worker پرداخت یا اعلان بود؟ نمی‌توانیم با اطمینان پاسخ دهیم؛ حدس زدن از روی ترتیب پنجره‌های console روش تحقیق نیست.

این مرحله عمداً فیلدهای قابل جست‌وجو، correlation بین سرویس‌ها، گراف trace و metric تجمیعی را ندارد.

## اجرا

`01-Opaque-System.slnx` را باز و AppHost را اجرا کنید. در `requests.http` یک درخواست `Normal` و یک درخواست `PaymentFailure` را نزدیک به هم بفرستید. خروجی می‌گوید پرداخت شکست خورد، اما هویت آن را نشان نمی‌دهد.

مرحلهٔ بعد `BookingId`، `PaymentId` و `Outcome` را به رکوردهای لاگ تبدیل می‌کند.

</div>
