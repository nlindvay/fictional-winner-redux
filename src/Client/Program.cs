using System.Diagnostics;
using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Serilog;
using Serilog.Events;
using static Common.OTelConfiguration;


var isRunningInContainer = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureMassTransitClient(isRunningInContainer);
builder.Services.ConfigureOtel(isRunningInContainer);
builder.Host.UseSerilog();

var app = builder.Build();
app.UseSerilogRequestLogging();

#region Order API
app.MapPost("order/", async ([FromServices] IRequestClient<OrderSubmitted> _client, SubmitOrder _request) =>
{
    var message = new OrderSubmitted(Guid.NewGuid(), DateTime.UtcNow, _request.PrimaryReference, _request.CustomerNumber, _request.PaymentCardNumber, _request.Notes, _request.OrderLines);
    var (accepted, exists, rejected) = await _client.GetResponse<OrderSubmissionAccepted, OrderSubmissionExists, OrderSubmissionRejected>(message);

    if (accepted.IsCompletedSuccessfully)
    {
        var response = await accepted;
        return Results.Ok(response.Message);
    }
    else if (exists.IsCompletedSuccessfully)
    {
        var response = await exists;
        return Results.Conflict(response.Message);
    }
    else
    {
        var response = await rejected;
        return Results.BadRequest(response.Message);
    }

}).WithTags("order").WithName("order-submit");

app.MapGet("order/{id}", async ([FromServices] IRequestClient<CheckOrderStatus> _client, Guid id) =>
{
    var message = new CheckOrderStatus(id);
    var (status, notFound) = await _client.GetResponse<OrderStatus, OrderNotFound>(message);

    if (status.IsCompletedSuccessfully)
    {
        var response = await status;
        return Results.Ok(response.Message);
    }
    else
    {
        var response = await notFound;
        return Results.NotFound(response.Message);
    }
}).WithTags("order").WithName("order-status");
#endregion

#region Customer API
app.MapPost("customer", async ([FromServices] IRequestClient<CustomerSubmitted> _client, SubmitCustomer _request) =>
{
    var message = new CustomerSubmitted(Guid.NewGuid(), DateTime.UtcNow, _request.CustomerNumber, _request.CustomerName);
    var (accepted, exists, rejected) = await _client.GetResponse<CustomerSubmissionAccepted, CustomerSubmissionExists, CustomerSubmissionRejected>(message);

    if (accepted.IsCompletedSuccessfully)
    {
        var response = await accepted;
        return Results.Ok(response.Message);
    }
    else if (exists.IsCompletedSuccessfully)
    {
        var response = await exists;
        return Results.Conflict(response.Message);
    }
    else
    {
        var response = await rejected;
        return Results.BadRequest(response.Message);
    }

}).WithTags("customer").WithName("customer-submit");

app.MapDelete("customer/{id}/action/orders/cancel", async ([FromServices] IPublishEndpoint _endpoint, Guid id, string customerNumber) =>
{
    var payload = new OrderCustomerAccountClosed(id, customerNumber);
    await _endpoint.Publish(payload);

    return Results.Accepted();

}).WithTags("customer").WithName("customer-cancel-orders");
#endregion


#region Product API
app.MapPost("product", async ([FromServices] IRequestClient<ProductSubmitted> _client, SubmitProduct _request) =>
{
    var message = new ProductSubmitted(Guid.NewGuid(), DateTime.UtcNow, _request.ProductCode, _request.CustomerNumber, _request.ProductName, _request.Quantity);
    var (accepted, exists, rejected) = await _client.GetResponse<ProductSubmissionAccepted, ProductSubmissionExists, ProductSubmissionRejected>(message);

    if (accepted.IsCompletedSuccessfully)
    {
        var response = await accepted;
        return Results.Ok(response.Message);
    }
    else if (exists.IsCompletedSuccessfully)
    {
        var response = await exists;
        return Results.Conflict(response.Message);
    }
    else
    {
        var response = await rejected;
        return Results.BadRequest(response.Message);
    }

}).WithTags("product").WithName("product-submit");
#endregion

app.UseSwagger();
app.UseSwaggerUI();
app.Run();
Log.CloseAndFlush();

