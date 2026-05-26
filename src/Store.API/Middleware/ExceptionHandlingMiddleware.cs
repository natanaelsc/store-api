using FluentValidation;
using Store.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Store.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException vex => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })),

            OrderNotFoundException => (
                HttpStatusCode.NotFound,
                "Order not found",
                exception.Message,
                null),

            DomainException => (
                HttpStatusCode.UnprocessableEntity,
                "Business rule violation",
                exception.Message,
                null),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Resource not found",
                exception.Message,
                null),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                "Please try again later.",
                null)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            errors,
            traceId = context.TraceIdentifier
        };

        string json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
