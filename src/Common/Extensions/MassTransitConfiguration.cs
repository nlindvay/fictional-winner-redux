using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

public static class MassTransitConfiguration
{
    public static IServiceCollection ConfigureMassTransitClient(this IServiceCollection services, bool isRunningInContainer)
    {
        services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();


                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(isRunningInContainer ? "rabbitmq" : "localhost", "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });
        return services;
    }

    public static IServiceCollection ConfigureMassTransitWorker(this IServiceCollection services, bool isRunningInContainer)
    {

        services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        x.SetMongoDbSagaRepositoryProvider(r =>
                        {
                            r.Connection = isRunningInContainer ? "mongodb://mongo" : "mongodb://127.0.0.1";
                            r.DatabaseName = "orders";
                        });

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly, typeof(OrderStateMachine).Assembly);
                        x.AddSagaStateMachines(entryAssembly, typeof(OrderStateMachine).Assembly);
                        x.AddSagas(entryAssembly, typeof(OrderStateMachine).Assembly);
                        x.AddActivities(entryAssembly, typeof(OrderStateMachine).Assembly);

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(isRunningInContainer ? "rabbitmq" : "localhost", "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });
        return services;
    }
}