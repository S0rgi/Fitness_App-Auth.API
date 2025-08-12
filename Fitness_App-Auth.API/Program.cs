using Fitness_App_Auth.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Fitness_App_Auth.API.Secure;
using DotNetEnv;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Service;
using Fitness_App_Auth.API.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Grpc.AspNetCore.Web;
// Загрузим .env (только локально)
DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// 👇 Добавляем поддержку переменных окружения поверх appsettings.json
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // <-- обязательно
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<INotificationPublisher>(sp =>
{
    var uriRabbitmq = builder.Configuration.GetConnectionString("RabbitMq");
    var pingUrl = builder.Configuration.GetConnectionString("PingNotifyUrl");
                Console.WriteLine(pingUrl);
    return new MessagePublisher(uriRabbitmq, pingUrl);
});

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
// JWT Аутентификация
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
builder.Services.AddScoped<IUsernameGenerator, UsernameGenerator>();

// регистрация уже есть как INotificationPublisher выше — не дублируем конкретный тип

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // API Key
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Введите API ключ в заголовке X-API-KEY",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "X-API-KEY",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    // Bearer JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Введите JWT Bearer токен в формате 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        },
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// Поддержка кастомного порта (для Fly)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listen =>
    {
        listen.Protocols = HttpProtocols.Http1AndHttp2;
    });
});


var app = builder.Build();

// Миграции
using ( var scope = app.Services.CreateScope() )
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UseRouting();
// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
     c.SwaggerEndpoint("../v1/swagger.json", "Auth API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseGrpcWeb(); // <-- Важно: до MapGrpcService!

app.MapControllers();

app.MapGrpcService<UserGrpcService>()
   .EnableGrpcWeb()         // Включаем поддержку gRPC-Web
   .AllowAnonymous();       // Если нужно разрешить анонимный доступ

app.MapGet("/", () => "Use a gRPC client to communicate");
app.MapGrpcReflectionService();

app.Run();
