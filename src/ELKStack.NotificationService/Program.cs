using ELKStack.NotificationService.State;
using ELKStack.Observability;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddElkStackObservability();
builder.Services.AddSingleton<NotificationState>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            cfg.Host(connectionString);
        }
        else
        {
            cfg.Host(
                builder.Configuration["RabbitMq:Host"] ?? "localhost",
                builder.Configuration["RabbitMq:VirtualHost"] ?? "/",
                h =>
                {
                    h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
                });
        }

        if (DemoStage.UsesCorrelation(builder.Configuration))
        {
            cfg.UseCorrelationFilters(context);
        }
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseElkStackObservability();
app.MapDefaultEndpoints();

app.UseAuthorization();

app.MapControllers();

app.Run();
