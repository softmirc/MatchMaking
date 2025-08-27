namespace MatchMaking.Service.Infrastructure
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionLoggingMiddleware> _logger;

        public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();

                await _next(httpContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Logging Middleware. CorrelationId: {correlationId}", httpContext.TraceIdentifier);

                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.Clear();
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    httpContext.Response.ContentType = "application/json";

                    var errorPayload = new
                    {
                        error = "An unexpected error occurred.",
                        correlationId = httpContext.TraceIdentifier
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(errorPayload);

                    await httpContext.Response.WriteAsync(json);
                }
            }
        }
    }
}
