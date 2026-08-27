using FluentValidation;
using System.Text.Json;
using TimeTracking.Application.Common;

namespace TimeTracking.Api;

/// <summary>
/// Маппинг исключений в ответы вида { code, message } с корректным HTTP-статусом.
/// Бизнес-правила — 400/409, ошибки валидации входа — 400, неожиданные — 500 (без деталей).
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // Клиент отменил запрос — это не ошибка сервера, ответ уже не нужен.
            if (!context.Response.HasStarted)
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (BusinessRuleException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (ValidationException ex)
        {
            var message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, message);
        }
        catch (BadHttpRequestException ex)
        {
            // Битый JSON / нечитаемое тело запроса — это 400, а не 500.
            _logger.LogInformation(ex, "Некорректное тело запроса");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ErrorCodes.ValidationError,
                "Некорректный формат тела запроса.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Внутренняя ошибка сервера.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int status, string code, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { code, message }));
    }
}
