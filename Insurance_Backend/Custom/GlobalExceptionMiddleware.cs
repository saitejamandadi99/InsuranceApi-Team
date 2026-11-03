using Insurance_Backend.Custom;
using Insurance_Backend.DTO.ResponseDTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Insurance_Backend.Exceptions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
                if (context.Response.HasStarted)
                    throw;

                var (statusCode, errorType, message) = ex switch
                {
                    BusinessException businessEx =>
                        (businessEx.StatusCode,
                         GetErrorType(businessEx.StatusCode),
                         businessEx.Message),

                    DbUpdateConcurrencyException =>
                        (StatusCodes.Status409Conflict,
                         "Concurrency Conflict",
                         "This record is being modified by another user. Please refresh and try again."),

                    KeyNotFoundException =>
                        (StatusCodes.Status404NotFound,
                         "Not Found",
                         ex.Message),

                    UnauthorizedAccessException =>
                        (StatusCodes.Status401Unauthorized,
                         "Unauthorized",
                         ex.Message),

                    _ =>
                        (StatusCodes.Status500InternalServerError,
                         "Internal Server Error",
                         "An unexpected error occurred. Please try again later.")
                };

                if (ex is BusinessException ||
                    ex is UnauthorizedAccessException ||
                    ex is KeyNotFoundException ||
                    ex is DbUpdateConcurrencyException)
                {
                    _logger.LogWarning(
                        ex,
                        "Request failed with status {StatusCode} at {Path}",
                        statusCode,
                        context.Request.Path);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception occurred while processing {Path}",
                        context.Request.Path);
                }

                var response = new ErrorResponseDTO
                {
                    TimeStamp = DateTime.UtcNow,
                    StatusCode = statusCode,
                    ErrorType = errorType,
                    Message = message,
                    RequestPath = context.Request.Path
                };

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, jsonOptions));
            }
        }

        private static string GetErrorType(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status422UnprocessableEntity =>
                    "Unprocessable Entity",
                StatusCodes.Status500InternalServerError =>
                    "Internal Server Error",
                _ => "Error"
            };
        }
    }
}