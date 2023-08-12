using MassTransit;
using MongoDB.Bson.Serialization.Attributes;

namespace Common;

public class CustomerState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public string CustomerName { get; set; }
    public string CustomerNumber { get; set; }
    public string CurrentState { get; set; }
    public string FaultReason { get; set; }
    public DateTime? SubmitDate { get; set; }
    public DateTime? Updated { get; set; }
    public int Version { get; set; }
    [BsonId]
    public Guid CorrelationId { get; set; }

}