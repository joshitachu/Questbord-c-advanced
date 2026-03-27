using System.Net;
using System.Text.Json;
using QuestBoard.Api.Exceptions;

namespace QuestBoard.Api.Middleware;

// [EIGEN INBRENG: Global Exception Handling Middleware]
// Centraal middleware-component dat alle onverwerkte exceptions opvangt en omzet
// naar gestructureerde JSON-responses met de juiste HTTP-statuscode.
// Voorkomt dat stack traces naar de client lekken bij onverwachte fouten (500).

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            QuestBoardNotFoundException notFound => (
                HttpStatusCode.NotFound,
                notFound.ErrorCode,
                notFound.Message
            ),
            InvalidQuestOperationException invalidOp => (
                HttpStatusCode.Conflict,
                invalidOp.ErrorCode,
                invalidOp.Message
            ),
            QuestBoardException questEx => (
                HttpStatusCode.BadRequest,
                questEx.ErrorCode,
                questEx.Message
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "Er is een onverwachte fout opgetreden."
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Onverwerkte exception opgevangen door GlobalExceptionHandler");
        }
        else
        {
            _logger.LogWarning("Bekende exception: {ErrorCode} — {Message}", errorCode, message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Error = errorCode,
            Message = message,
            StatusCode = (int)statusCode
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
