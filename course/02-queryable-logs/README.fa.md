<div dir="rtl">

# ۰۲ — لاگ‌هایی که واقعاً می‌توان جست‌وجو کرد

[English](README.md)

## لاگ و رکورد لاگ چیست؟

لاگ شاهدی از یک اتفاق در زمان اجراست. رکورد لاگ علاوه بر جملهٔ قابل خواندن، معمولاً timestamp، level، category، exception، properties نام‌دار و context سرویس/درخواست را دارد. console ممکن است آن را یک خط نشان دهد، اما backend می‌تواند هر فیلد را جداگانه ایندکس و جست‌وجو کند.

## logging در .NET

کد برنامه به abstraction استاندارد `Microsoft.Extensions.Logging` وابسته است:

```csharp
public sealed class PaymentRequestedConsumer(
    ILogger<PaymentRequestedConsumer> logger)
{
}
```

`ILogger<T>` یک category می‌سازد. `ILoggerFactory` رکورد را به providerهای ثبت‌شده می‌فرستد: console، OTLP، فایل یا backend دیگر. بنابراین کد کسب‌وکاری می‌گوید «چه اتفاقی افتاد» و composition تعیین می‌کند «کجا برود».

ASP.NET Core با `WebApplication.CreateBuilder(args)` configuration، dependency injection و logging را از ابتدا متصل می‌کند. Controller، middleware و consumer می‌توانند `ILogger<T>` را inject کنند. تنظیم `Logging:LogLevel` در `appsettings.json` سطح و categoryهای خروجی را کنترل می‌کند.

## Serilog در این مخزن

Serilog جای abstraction .NET را نمی‌گیرد؛ implementation آن است. `ConfigureSerilog` در Service Defaults، console، enrichment از `LogContext`، نام سرویس/environment و در صورت وجود endpoint، خروجی OTLP را پیکربندی می‌کند. `UseSerilogRequestLogging` برای هر درخواست HTTP یک رکورد completion با status و duration ثبت می‌کند.

## چرا structured logging لازم است؟

از interpolation پرهیز کنید:

```csharp
logger.LogInformation($"Processing booking {booking.Id}");
```

به‌جایش message template بنویسید:

```csharp
logger.LogInformation("Processing booking {BookingId}", booking.Id);
```

در حالت دوم `BookingId` یک property تایپ‌دار و قابل فیلتر است، نه بخشی از یک جمله. نام فیلدها باید پایدار و معنایی باشند و هرگز token، رمز یا دادهٔ کارت پرداخت در لاگ قرار نگیرد.

هنوز مشکل باقی است: لاگ‌های ساخت‌یافته می‌گویند هر رکورد چیست، اما نمی‌گویند کدام رکوردها متعلق به یک عملیات توزیع‌شده‌اند.

</div>
