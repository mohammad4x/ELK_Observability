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

var flightBookingGateway = builder.AddYarp("elkstack-flight-booking-gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/bookings/{**catch-all}", bookingService);
        yarp.AddRoute("/api/payments/{**catch-all}", paymentService);
        yarp.AddRoute("/api/notifications/{**catch-all}", notificationService);
    })
    .WaitFor(bookingService)
    .WaitFor(paymentService)
    .WaitFor(notificationService)
    .WithOtlpExporter();

var flightBookingWeb = builder.AddJavaScriptApp("elkstack-flight-booking-web", "../../src/ELKStack.FlightBooking.Web", "dev")
    .WithEnvironment("BOOKING_SERVICE_URL", flightBookingGateway.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT")
    .WaitFor(flightBookingGateway)
    .WithExternalHttpEndpoints()
    .WithOpenTelemetryCollectorRouting(observability.OtelCollector);

bookingService.WaitFor(observability.OtelCollector);
paymentService.WaitFor(observability.OtelCollector);
notificationService.WaitFor(observability.OtelCollector);
flightBookingWeb.WaitFor(observability.OtelCollector);
flightBookingGateway.WaitFor(observability.OtelCollector);

builder.Build().Run();
