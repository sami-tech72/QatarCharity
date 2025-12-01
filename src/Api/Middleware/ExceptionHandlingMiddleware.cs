using System.Net;
using System.Text.Json;
using Api.Models;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "An unhandled exception occurred after the response started.");
                throw;
            }

            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");

            var failureResponse = CreateFailureResponse(context);

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, failureResponse, serializerOptions);
        }
    }

    private static ApiResponse<object> CreateFailureResponse(HttpContext context)
    {
        var details = new Dictionary<string, object?>
        {
            ["traceId"] = context.TraceIdentifier,
            ["path"] = context.Request.Path,
            ["method"] = context.Request.Method
        };

        return ApiResponse<object>.Fail(
            message: "An unexpected error occurred while processing the request.",
            errorCode: "unexpected_error",
            details: details);
    }
}
