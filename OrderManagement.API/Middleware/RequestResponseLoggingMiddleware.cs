
using System.Diagnostics;
using System.Text;

namespace OrderManagement.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var method = context.Request.Method;
        var path = context.Request.Path;

        if (path.StartsWithSegments("/api/auth"))
        {
            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "{Method} {Path} | Status: {StatusCode} | Time: {Elapsed}ms",
                method, path, context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return;
        }

        var requestBody = await ReadRequestAsync(context.Request);

        var originalBody = context.Response.Body;

        await using var responseStream = new MemoryStream();

        try
        {
            context.Response.Body = responseStream;

            await _next(context);

            responseStream.Position = 0;

            var responseBody = await new StreamReader(responseStream)
                .ReadToEndAsync();

            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} | Request: {RequestBody} | Response: {ResponseBody} | Status: {StatusCode} | Time: {Elapsed}ms",
                method,
                path,
                requestBody,
                responseBody,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static async Task<string> ReadRequestAsync(HttpRequest request)
    {
        if (request.ContentType?.Contains("application/json") != true)
            return string.Empty;

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;

        return body;
    }
}
