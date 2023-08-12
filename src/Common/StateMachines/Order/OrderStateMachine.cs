namespace Common;

using System;
using System.Linq;
using MassTransit;

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderSubmitted, x =>
        {
            x.CorrelateBy((saga, context) => saga.PrimaryReference == context.Message.PrimaryReference && saga.CustomerNumber == context.Message.CustomerNumber).SelectId(context => NewId.NextGuid());
        });

        Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => FulfillmentCompleted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => FulfillmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => FulfillOrderFaulted, x => x.CorrelateById(m => m.Message.Message.OrderId));
        Event(() => OrderStatusRequested, x =>
        {
            x.CorrelateById(m => m.Message.OrderId);
            x.OnMissingInstance(m => m.ExecuteAsync(async context =>
            {
                if (context.RequestId.HasValue)
                {
                    await context.RespondAsync<OrderNotFound>(new { context.Message.OrderId });
                }
            }));
        });
        Event(() => AccountClosed, x => x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber));

        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .Then(async context =>
                {
                    context.Saga.CorrelationId = NewId.NextGuid();
                    context.Saga.SubmitDate = DateTime.UtcNow;
                    context.Saga.PrimaryReference = context.Message.PrimaryReference;
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.PaymentCardNumber = context.Message.PaymentCardNumber;
                    context.Saga.Updated = DateTime.UtcNow;
                    context.Saga.OrderLines = context.Message.OrderLines;

                    await context.RespondAsync(new OrderSubmissionAccepted(context.Saga.CorrelationId, context.Message.Timestamp, context.Message.PrimaryReference, context.Message.CustomerNumber, "all good"));
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderSubmitted)
                .Then(async context => await context.RespondAsync(new OrderSubmissionExists(context.Saga.CorrelationId))),
            When(AccountClosed)
                .TransitionTo(Canceled),
            When(OrderAccepted)
                .Activity(x => x.OfType<AcceptOrderActivity>())
                .TransitionTo(Accepted));

        During(Accepted,
            When(FulfillOrderFaulted)
                .Then(context => Console.WriteLine("Fulfill Order Faulted: {0}", context.Message.Exceptions.FirstOrDefault()?.Message))
                .TransitionTo(Faulted),
            When(FulfillmentFaulted)
                .TransitionTo(Faulted),
            When(FulfillmentCompleted)
                .TransitionTo(Completed));

        During(Canceled,
            Ignore(AccountClosed),
            Ignore(OrderSubmitted),
            Ignore(OrderAccepted),
            Ignore(FulfillmentFaulted),
            Ignore(FulfillmentCompleted),
            Ignore(FulfillOrderFaulted),
            Ignore(OrderStatusRequested));

        DuringAny(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.SubmitDate ??= context.Message.Timestamp;
                    context.Saga.CustomerNumber ??= context.Message.CustomerNumber;
                }),
            When(OrderStatusRequested)
                .Then(async context => await context.RespondAsync(new OrderStatus(context.Saga.CorrelationId, context.Saga.CurrentState)))
        );
    }

    public State Submitted { get; private set; }
    public State Accepted { get; private set; }
    public State Canceled { get; private set; }
    public State Faulted { get; private set; }
    public State Completed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<OrderAccepted> OrderAccepted { get; private set; }
    public Event<OrderFulfillmentCompleted> FulfillmentCompleted { get; private set; }
    public Event<OrderFulfillmentFaulted> FulfillmentFaulted { get; private set; }
    public Event<CheckOrderStatus> OrderStatusRequested { get; private set; }
    public Event<OrderCustomerAccountClosed> AccountClosed { get; private set; }
    public Event<Fault<FulfillOrder>> FulfillOrderFaulted { get; private set; }
}
