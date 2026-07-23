# 02 — Queryable logs

See [the lesson guide](../../docs/02-structured-logs.md). This stage begins
with log records and the .NET logging pipeline, then introduces Serilog and
structured properties while deliberately keeping cross-service correlation off.

## Code to show live

1. `Aspire/ELKStack.ServiceDefaults/Extensions.cs`
   - `ConfigureSerilog`
   - `UseSerilogRequestLogging`
2. `src/ELKStack.PaymentService/Consumers/PaymentRequestedConsumer.cs`
   - the `ILogger<T>` dependency and structured payment outcome record
3. `appsettings.json` in any service
   - category-level `Logging:LogLevel` configuration
