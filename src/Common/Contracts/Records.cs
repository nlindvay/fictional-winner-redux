namespace Common;

#region Order contracts
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
#endregion

#region Customer contracts
public record SubmitCustomer(string CustomerNumber, string CustomerName);
public record CustomerSubmitted(Guid CustomerId, DateTime Timestamp, string CustomerNumber, string CustomerName);
public record CustomerSubmissionAccepted(Guid CustomerId, DateTime Timestamp, string CustomerNumber);
public record CustomerSubmissionExists(Guid CustomerId);
public record CustomerSubmissionRejected(Guid CustomerId, DateTime Timestamp, string CustomerNumber, string Reason);
#endregion

#region Product contracts
public record SubmitProduct(string ProductCode, string CustomerNumber, string ProductName, int Quantity);
public record ProductSubmitted(Guid ProductId, DateTime Timestamp, string ProductCode, string CustomerNumber, string ProductName, int Quantity);
public record ProductSubmissionAccepted(Guid ProductId, DateTime Timestamp, string ProductCode);
public record ProductSubmissionExists(Guid ProductId);
public record ProductSubmissionRejected(Guid ProductId, DateTime Timestamp, string ProductCode, string Reason);
public record CheckProductStatus(Guid ProductId);
public record ProductNotFound(Guid ProductId);
public record ProductStatus(Guid ProductId, string State);
#endregion