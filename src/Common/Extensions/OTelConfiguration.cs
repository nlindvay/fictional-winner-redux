using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common;

public static class OTelConfiguration
{
    public static IServiceCollection ConfigureOtel(this IServiceCollection services, bool isRunningInContainer)
    {

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
                tracerProviderBuilder
                    .AddSource("MassTransit")
                    .AddSource(Assembly.GetEntryAssembly()?.GetName().Name)
                    .ConfigureResource(resource => resource
                        .AddService(Assembly.GetEntryAssembly()?.GetName().Name)
                        .AddTelemetrySdk())
                    .AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = isRunningInContainer ? "jaeger" : "localhost";
                        o.AgentPort = 6831;
                        o.ExportProcessorType = ExportProcessorType.Batch;
                        o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512,
                        };
                    }));


        return services;
    }


}
