using MassTransit;
using MongoDB.Bson.Serialization.Attributes;

namespace Common;

public class ProductState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public string ProductName { get; set; }
    public string ProductCode { get; set; }
    public int Quantity { get; set; }
    public string CustomerNumber { get; set; }
    public string CurrentState { get; set; }
    public string FaultReason { get; set; }
    public DateTime? SubmitDate { get; set; }
    public DateTime? Updated { get; set; }
    public int Version { get; set; }
    [BsonId]
    public Guid CorrelationId { get; set; }

}