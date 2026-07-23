using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

#if STAGE_TRACING || STAGE_METRICS || STAGE_PIPELINE || STAGE_COMPLETE
var observability = builder.AddElasticApmStack();
#endif

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var bookingService = builder.AddProject<Projects.ELKStack_BookingService>("booking-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", CourseStage.Name)
    .WithExternalHttpEndpoints();

var paymentService = builder.AddProject<Projects.ELKStack_PaymentService>("payment-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", CourseStage.Name)
    .WithExternalHttpEndpoints();

var notificationService = builder.AddProject<Projects.ELKStack_NotificationService>("notification-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", CourseStage.Name)
    .WithExternalHttpEndpoints();

#if STAGE_TRACING || STAGE_METRICS || STAGE_PIPELINE || STAGE_COMPLETE
bookingService.WaitFor(observability.OtelCollector);
paymentService.WaitFor(observability.OtelCollector);
notificationService.WaitFor(observability.OtelCollector);
#endif

builder.Build().Run();

internal static class CourseStage
{
#if STAGE_STRUCTURED_LOGGING
    public const string Name = "StructuredLogging";
#elif STAGE_CORRELATION
    public const string Name = "Correlation";
#elif STAGE_TRACING
    public const string Name = "Tracing";
#elif STAGE_METRICS
    public const string Name = "Metrics";
#elif STAGE_PIPELINE
    public const string Name = "Pipeline";
#else
    public const string Name = "Complete";
#endif
}
