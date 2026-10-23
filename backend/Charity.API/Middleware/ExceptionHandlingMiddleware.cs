using System.Net;
using System.Text.Json;
using Charity.Domain.Exceptions;

namespace Charity.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            BalanceConfirmationRequiredException balance => (HttpStatusCode.Conflict, balance.Message),
            UnauthorizedDomainException unauth => (HttpStatusCode.Forbidden, unauth.Message),
            EntityNotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            ValidationDomainException valEx => (HttpStatusCode.BadRequest, valEx.Message),
            _ => (HttpStatusCode.InternalServerError, "حدث خطأ غير متوقع في الخادم. يرجى المحاولة لاحقاً.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message,
            code = exception is BalanceConfirmationRequiredException ? "balance_confirmation_required" : null,
            timestamp = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
