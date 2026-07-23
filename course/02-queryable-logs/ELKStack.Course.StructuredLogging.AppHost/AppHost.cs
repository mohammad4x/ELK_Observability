using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var bookingService = builder.AddProject<Projects.ELKStack_BookingService>("booking-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", "StructuredLogging")
    .WithExternalHttpEndpoints();

var paymentService = builder.AddProject<Projects.ELKStack_PaymentService>("payment-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", "StructuredLogging")
    .WithExternalHttpEndpoints();

var notificationService = builder.AddProject<Projects.ELKStack_NotificationService>("notification-service")
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment("Demo__Stage", "StructuredLogging")
    .WithExternalHttpEndpoints();

builder.Build().Run();
