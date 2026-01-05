using System.Net;
using System.Text.Json;

namespace basics.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Tüm yakalanmamış hataları yakalar, loglar ve kullanıcıya şık bir hata sayfası gösterir.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
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
            // Benzersiz hata ID'si oluştur (kullanıcıya gösterilecek ve logda aranabilir)
            var errorId = Guid.NewGuid().ToString("N")[..8].ToUpper();

            // Hata detaylarını logla
            _logger.LogError(
                exception,
                "Hata ID: {ErrorId} | URL: {Url} | Method: {Method} | User: {User} | IP: {IP}",
                errorId,
                context.Request.Path + context.Request.QueryString,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous",
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            // Dosyaya da logla
            await LogToFileAsync(errorId, context, exception);

            // HTTP Status Code belirleme
            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            // AJAX/API isteği mi kontrol et
            if (IsAjaxRequest(context))
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    success = false,
                    message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.",
                    errorId = errorId
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            else
            {
                // Normal isteklerde hata sayfasına yönlendir
                context.Response.Redirect($"/Home/Error?errorId={errorId}");
            }
        }

        private bool IsAjaxRequest(HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   context.Request.Headers["Accept"].ToString().Contains("application/json");
        }

        private async Task LogToFileAsync(string errorId, HttpContext context, Exception exception)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
                Directory.CreateDirectory(logDirectory);

                var logFileName = $"error_{DateTime.Now:yyyy-MM-dd}.log";
                var logFilePath = Path.Combine(logDirectory, logFileName);

                var logEntry = $"""
                    ═══════════════════════════════════════════════════════════════
                    Hata ID: {errorId}
                    Tarih: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    ───────────────────────────────────────────────────────────────
                    URL: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                    User: {context.User?.Identity?.Name ?? "Anonymous"}
                    IP: {context.Connection.RemoteIpAddress}
                    User-Agent: {context.Request.Headers["User-Agent"]}
                    ───────────────────────────────────────────────────────────────
                    Exception Type: {exception.GetType().FullName}
                    Message: {exception.Message}
                    ───────────────────────────────────────────────────────────────
                    Stack Trace:
                    {exception.StackTrace}
                    ───────────────────────────────────────────────────────────────
                    Inner Exception: {exception.InnerException?.Message ?? "None"}
                    ═══════════════════════════════════════════════════════════════

                    """;

                await File.AppendAllTextAsync(logFilePath, logEntry);
            }
            catch (Exception logEx)
            {
                // Logging hatası - sadece console'a yaz
                _logger.LogWarning(logEx, "Hata dosyaya yazılamadı");
            }
        }
    }

    /// <summary>
    /// Middleware extension method
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
