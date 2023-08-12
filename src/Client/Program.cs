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

// submit order

app.MapPost("order/", async ([FromServices] IRequestClient<OrderSubmitted> _client, SubmitOrder _request) =>
{
    var orderSubmitted = new OrderSubmitted(Guid.NewGuid(), DateTime.UtcNow, _request.PrimaryReference, _request.CustomerNumber, _request.PaymentCardNumber, _request.Notes, _request.OrderLines);

    var (accepted, exists, rejected) = await _client.GetResponse<OrderSubmissionAccepted, OrderSubmissionExists, OrderSubmissionRejected>(orderSubmitted);

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

// get order status

app.MapGet("order/{id}", async ([FromServices] IRequestClient<CheckOrderStatus> _client, Guid id) =>
{
    var (status, notFound) = await _client.GetResponse<OrderStatus, OrderNotFound>(new(id));

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

// cancel order

app.MapDelete("customer/{id}", async ([FromServices] IPublishEndpoint _endpoint, Guid id, string customerNumber) =>
{
    await _endpoint.Publish<OrderCustomerAccountClosed>(new(id, customerNumber));

    return Results.Accepted();

}).WithTags("order").WithName("order-cancel");



app.UseSwagger();
app.UseSwaggerUI();
app.Run();
Log.CloseAndFlush();

