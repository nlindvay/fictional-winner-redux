namespace Common;

public record OrderLine(Guid OrderLineId, string ProductCode, int Quantity);
public record SubmitOrder(string PrimaryReference, string CustomerNumber, string PaymentCardNumber, string Notes, OrderLine[] OrderLines);
public record OrderSubmissionRejected(string PrimaryReference, string CustomerNumber, string Reason);
public record OrderSubmissionExists(Guid OrderId);
public record OrderSubmissionAccepted(Guid OrderId, DateTime Timestamp, string PrimaryReference, string CustomerNumber, string Reason);
public record CheckOrderStatus(Guid OrderId);
public record OrderStatus(Guid OrderId, string State);
public record OrderNotFound(Guid OrderId);
public record OrderSubmitted(Guid OrderId, DateTime Timestamp, string PrimaryReference, string CustomerNumber, string PaymentCardNumber, string Notes, OrderLine[] OrderLines);
public record OrderAccepted(Guid OrderId, DateTime Timestamp);
public record FulfillOrder(Guid OrderId, string CustomerNumber, string PaymentCardNumber);
public record OrderFulfillmentFaulted(Guid OrderId, DateTime Timestamp);
public record OrderFulfillmentCompleted(Guid OrderId, DateTime Timestamp);
public record OrderCustomerAccountClosed(Guid CustomerId, string CustomerNumber);