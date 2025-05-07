namespace Fitness_App_Auth.API.Secure;
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
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
