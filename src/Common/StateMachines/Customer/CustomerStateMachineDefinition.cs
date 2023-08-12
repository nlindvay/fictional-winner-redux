namespace Common;

using MassTransit;

public class CustomerStateMachineDefinition :
    SagaDefinition<CustomerState>
{
    public CustomerStateMachineDefinition()
    {
        ConcurrentMessageLimit = 12;
    }

    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<CustomerState> sagaConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 10000));
        endpointConfigurator.UseInMemoryOutbox();
    }
}
