using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var observability = builder.AddElasticApmStack();

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var bookingService = builder.AddProject<Projects.ELKStack_BookingService>("elkstack-booking-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints();

var paymentService = builder.AddProject<Projects.ELKStack_PaymentService>("elkstack-payment-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints();

var notificationService = builder.AddProject<Projects.ELKStack_NotificationService>("elkstack-notification-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints();

var flightBookingWeb = builder.AddJavaScriptApp("elkstack-flight-booking-web", "../../src/ELKStack.FlightBooking.Web", "dev")
    .WithReference(bookingService)
    .WithEnvironment("BOOKING_SERVICE_URL", bookingService.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT")
    .WaitFor(bookingService)
    .WithExternalHttpEndpoints()
    .WithOpenTelemetryCollectorRouting(observability.OtelCollector);

bookingService.WaitFor(observability.OtelCollector);
paymentService.WaitFor(observability.OtelCollector);
notificationService.WaitFor(observability.OtelCollector);
flightBookingWeb.WaitFor(observability.OtelCollector);

builder.Build().Run();
