namespace Common;

using System;
using System.Linq;
using MassTransit;

public class CustomerStateMachine :
    MassTransitStateMachine<CustomerState>
{
    public CustomerStateMachine()
    {
        Event(() => CustomerSubmitted, x =>
        {
            x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber).SelectId(context => NewId.NextGuid());
        });

        InstanceState(x => x.CurrentState);

        Initially(
            When(CustomerSubmitted)
                .Then(async context =>
                {
                    context.Saga.SubmitDate = DateTime.UtcNow;
                    context.Saga.Updated = DateTime.UtcNow;
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.CustomerName = context.Message.CustomerName;
                    await context.RespondAsync(new CustomerSubmissionAccepted(context.Saga.CorrelationId, context.Saga.SubmitDate.Value, context.Message.CustomerNumber));
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(CustomerSubmitted)
                .Then(async context => await context.RespondAsync(new CustomerSubmissionExists(context.Saga.CorrelationId))));


        DuringAny(
            When(CustomerSubmitted)
                .Then(context =>
                {
                    context.Saga.SubmitDate ??= context.Message.Timestamp;
                    context.Saga.CustomerNumber ??= context.Message.CustomerNumber;
                }));
    }

    public State Submitted { get; private set; }
    public State Faulted { get; private set; }

    public Event<CustomerSubmitted> CustomerSubmitted { get; private set; }
}
