namespace Common;

using System;
using System.Linq;
using MassTransit;

public class ProductStateMachine :
    MassTransitStateMachine<ProductState>
{
    public ProductStateMachine()
    {
        Event(() => ProductSubmitted, x =>
        {
            x.CorrelateBy((saga, context) => saga.ProductCode == context.Message.ProductCode && saga.CustomerNumber == context.Message.CustomerNumber).SelectId(context => NewId.NextGuid());
        });

        InstanceState(x => x.CurrentState);

        Initially(
            When(ProductSubmitted)
                .Then(async context =>
                {
                    context.Saga.SubmitDate = DateTime.UtcNow;
                    context.Saga.Updated = DateTime.UtcNow;
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.ProductCode = context.Message.ProductCode;
                    context.Saga.ProductName = context.Message.ProductName;
                    context.Saga.Quantity = context.Message.Quantity;
                    await context.RespondAsync(new ProductSubmissionAccepted(context.Saga.CorrelationId, context.Saga.SubmitDate.Value, context.Message.ProductCode));
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(ProductSubmitted)
                .Then(async context => await context.RespondAsync(new ProductSubmissionExists(context.Saga.CorrelationId))));


        DuringAny(
            When(ProductSubmitted)
                .Then(context =>
                {
                    context.Saga.SubmitDate ??= context.Message.Timestamp;
                    context.Saga.CustomerNumber ??= context.Message.CustomerNumber;
                }));
    }

    public State Submitted { get; private set; }
    public State Faulted { get; private set; }

    public Event<ProductSubmitted> ProductSubmitted { get; private set; }
}
