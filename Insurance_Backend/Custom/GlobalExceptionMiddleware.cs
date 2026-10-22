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
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception occurred.");

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        statusCode = ex.StatusCode,
                        message = ex.Message
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred.");

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        statusCode = 500,
                        message = "An unexpected error occurred."
                    }));
            }
        }
    }
}