
using Microsoft.AspNetCore.Mvc;

namespace OrderManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred at {Path}. TraceId: {TraceId}",
                context.Request.Path,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            KeyNotFoundException => 404,
            ArgumentException => 400,
            InvalidOperationException => 400,
            UnauthorizedAccessException => 403,
            _ => 500
        };

        var response = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == 500
                ? "Internal Server Error"
                : "Request Failed",
            Detail = statusCode == 500
                ? "An unexpected error occurred."
                : ex.Message,
            Instance = context.Request.Path
        };

        response.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(response);
    }
}
