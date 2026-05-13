namespace Aspire.Hosting;

public static class ObservabilityExtensions
{
    private const string ElasticVersion = "8.17.3";

    private const string OtelCollectorConfig = """
        receivers:
          otlp:
            protocols:
              grpc:
                endpoint: 0.0.0.0:4317
              http:
                endpoint: 0.0.0.0:4318

        processors:
          batch:
            timeout: 1s
            send_batch_size: 1024
          memory_limiter:
            check_interval: 1s
            limit_mib: 512
            spike_limit_mib: 128

        exporters:
          debug:
            verbosity: basic
          otlp/apm:
            endpoint: ${env:APM_SERVER_ENDPOINT}
            tls:
              insecure: true

        service:
          pipelines:
            traces:
              receivers: [otlp]
              processors: [memory_limiter, batch]
              exporters: [debug, otlp/apm]
            metrics:
              receivers: [otlp]
              processors: [memory_limiter, batch]
              exporters: [debug, otlp/apm]
            logs:
              receivers: [otlp]
              processors: [memory_limiter, batch]
              exporters: [debug, otlp/apm]
        """;

    public static ObservabilityResources AddElasticApmStack(this IDistributedApplicationBuilder builder)
    {
        var elasticsearch = builder.AddElasticsearch("elasticsearch")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("discovery.type", "single-node")
            .WithDataVolume()
            .WithLifetime(ContainerLifetime.Persistent);

        var apmServer = builder.AddContainer("apm-server", "elastic/apm-server", ElasticVersion)
            .WithEnvironment("ELASTIC_APM_SECRET_TOKEN", "")
            .WithEnvironment("output.elasticsearch.hosts", elasticsearch.GetEndpoint("http"))
            .WithArgs("--strict.perms=false")
            .WithHttpEndpoint(targetPort: 8200, name: "http")
            .WithLifetime(ContainerLifetime.Persistent)
            .WaitFor(elasticsearch);

        var kibana = builder.AddContainer("kibana", "elastic/kibana", ElasticVersion)
            .WithEnvironment("ELASTICSEARCH_HOSTS", elasticsearch.GetEndpoint("http"))
            .WithHttpEndpoint(targetPort: 5601, name: "http")
            .WithLifetime(ContainerLifetime.Persistent)
            .WaitFor(elasticsearch);

        var otelCollector = builder.AddOpenTelemetryCollector("otel-collector")
            .WithEnvironment("APM_SERVER_ENDPOINT", apmServer.GetEndpoint("http"))
            .WithConfig(WriteOtelCollectorConfig())
            .WithAppForwarding()
            .WithLifetime(ContainerLifetime.Persistent)
            .WaitFor(apmServer);

        return new ObservabilityResources(elasticsearch, apmServer, kibana, otelCollector);
    }

    private static string WriteOtelCollectorConfig()
    {
        var configDirectory = Path.Combine(Path.GetTempPath(), "elkstack-otel");
        Directory.CreateDirectory(configDirectory);

        var configPath = Path.Combine(configDirectory, "otel-collector-config.yaml");
        File.WriteAllText(configPath, OtelCollectorConfig);

        return configPath;
    }
}

public sealed class ObservabilityResources(
    IResourceBuilder<ElasticsearchResource> elasticsearch,
    IResourceBuilder<ContainerResource> apmServer,
    IResourceBuilder<ContainerResource> kibana,
    IResourceBuilder<OpenTelemetryCollectorResource> otelCollector)
{
    public IResourceBuilder<ElasticsearchResource> Elasticsearch { get; } = elasticsearch;
    public IResourceBuilder<ContainerResource> ApmServer { get; } = apmServer;
    public IResourceBuilder<ContainerResource> Kibana { get; } = kibana;
    public IResourceBuilder<OpenTelemetryCollectorResource> OtelCollector { get; } = otelCollector;
}
