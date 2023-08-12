using System;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        await CreateHostBuilder(args).Build().RunAsync();

        Log.CloseAndFlush();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var isRunningInContainer = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;
                services.ConfigureMassTransitWorker(isRunningInContainer);
                services.ConfigureOtel(isRunningInContainer);
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.AddSerilog(dispose: true);
            });
}

