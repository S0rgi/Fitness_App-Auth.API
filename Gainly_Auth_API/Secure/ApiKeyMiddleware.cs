using Microsoft.Extensions.Hosting;
namespace Gainly_Auth_API.Secure;
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment)
    {
        _next = next;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
         if (context.Request.ContentType?.StartsWith("application/grpc") == true)
    {
        await _next(context); // Пропускаем проверку для gRPC
        return;
    }

        // В dev-окружении пропускаем Swagger/health
        if (_environment.IsDevelopment())
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || path == "/" || path.StartsWith("/health") || path.EndsWith("/swagger.json"))
            {
                await _next(context);
                return;
            }
        }

        var apiKeyHeader = _configuration["ApiKey:HeaderName"];
        var configuredApiKey = _configuration["ApiKey:Key"];

        if (context.Request.Headers.TryGetValue(apiKeyHeader, out var extractedApiKey))
        {
            if (extractedApiKey == configuredApiKey)
            {
                await _next(context);
                return;
            }
        }

        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized: Invalid or missing API Key.");
    }
}



