using Aspire.Hosting;
using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var observability = builder.AddElasticApmStack();

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var bookingService = builder.AddProject<Projects.ELKStack_BookingService>("elkstack-booking-service", launchProfileName: "http")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints();

var paymentService = builder.AddProject<Projects.ELKStack_PaymentService>("elkstack-payment-service", launchProfileName: "http")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints();

var notificationService = builder.AddProject<Projects.ELKStack_NotificationService>("elkstack-notification-service", launchProfileName: "http")
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

var rumIngress = builder.AddYarp("elkstack-rum-ingress")
    .WithConfiguration(yarp =>
        yarp.AddRoute("/rum", observability.ApmServer.GetEndpoint("http"))
            .WithMatchMethods("POST", "OPTIONS")
            .WithTransformPathSet("/intake/v3/rum/events"))
    .WaitFor(observability.ApmServer)
    .WithExternalHttpEndpoints()
    .WithOtlpExporter();

var flightBookingWeb = builder.AddJavaScriptApp("elkstack-flight-booking-web", "../../src/ELKStack.FlightBooking.Web", "dev")
    .WithEnvironment("BOOKING_SERVICE_URL", flightBookingGateway.GetEndpoint("http"))
    .WithEnvironment("NEXT_PUBLIC_ELASTIC_APM_SERVER_URL", rumIngress.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT")
    .WaitFor(flightBookingGateway)
    .WaitFor(rumIngress)
    .WithExternalHttpEndpoints()
    .WithOpenTelemetryCollectorRouting(observability.OtelCollector);

bookingService.WaitFor(observability.OtelCollector);
paymentService.WaitFor(observability.OtelCollector);
notificationService.WaitFor(observability.OtelCollector);
flightBookingWeb.WaitFor(observability.OtelCollector);
flightBookingGateway.WaitFor(observability.OtelCollector);
rumIngress.WaitFor(observability.OtelCollector);

builder.Build().Run();
