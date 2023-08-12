namespace Common;

using System;
using System.Threading.Tasks;
using MassTransit;


public class AcceptOrderActivity :
    IStateMachineActivity<OrderState, OrderAccepted>
{
    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context, IBehavior<OrderState, OrderAccepted> next)
    {
        Console.WriteLine("Hello, World. Order is {0}", context.Message.OrderId);

        var consumeContext = context.GetPayload<ConsumeContext>();

        var sendEndpoint = await consumeContext.GetSendEndpoint(new Uri("queue:fulfill-order"));

        await sendEndpoint.Send<FulfillOrder>(new
        {
            context.Message.OrderId,
            context.Saga.CustomerNumber,
            context.Saga.PaymentCardNumber,
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context, IBehavior<OrderState, OrderAccepted> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("accept-order");
    }
}
