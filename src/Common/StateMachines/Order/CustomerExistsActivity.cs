using MassTransit;

namespace Common;

public class CustomerExistsActivity : IStateMachineActivity<CustomerState, OrderSubmitted>
{
    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public Task Execute(BehaviorContext<CustomerState, OrderSubmitted> context, IBehavior<CustomerState, OrderSubmitted> next)
    {
        throw new NotImplementedException();
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CustomerState, OrderSubmitted, TException> context, IBehavior<CustomerState, OrderSubmitted> next) where TException : Exception
    {
        throw new NotImplementedException();
    }

    public void Probe(ProbeContext context)
    {
        throw new NotImplementedException();
    }
}
