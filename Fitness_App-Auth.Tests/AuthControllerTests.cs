using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Fitness_App_Auth.API.Controllers;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Models;
using Fitness_App_Auth.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Fitness_App_Auth.API.Data;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Fitness_App_Auth.Test;
public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUsernameGenerator> _usernameGenMock = new();
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _dbContext;

    private readonly string _testEmail;
    private readonly string _testUsername;

    public AuthControllerTests()
    {
        // Конфигурация
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "supersecretkeyforjwt123456781290"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // In-memory DB
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AuthDbContext(options);

        // Email & username
        _testEmail = $"test_{Guid.NewGuid()}@mail.com";
        _testUsername = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _usernameGenMock.Setup(x => x.GenerateAsync(It.IsAny<string>())).ReturnsAsync(_testUsername);

        // Возвращаем токены при регистрации/входе
        _authServiceMock.Setup(x => x.GenerateTokensAsync(It.IsAny<User>()))
            .ReturnsAsync(("testAccessToken", "testRefreshToken"));

        _controller = new AuthController(_dbContext, _configuration, _tokenServiceMock.Object, _authServiceMock.Object, _usernameGenMock.Object);
    }

    [Fact]
    public async Task Full_Auth_Flow()
    {
        // Проверка отсутствия юзера
        var existsResult = await _controller.UserExist(_testEmail);
        Assert.IsType<BadRequestObjectResult>(existsResult);

        // Регистрация
        var registerResult = await _controller.Register(new RegisterDto
        {
            Email = _testEmail,
            Password = "StrongPass123"
        });
        var okRegister = Assert.IsType<OkObjectResult>(registerResult);
        Assert.NotNull(okRegister.Value);

        // Логин
        var loginResult = await _controller.LoginAsync(new LoginDto
        {
            Email = _testEmail,
            Password = "StrongPass123"
        });
        var okLogin = Assert.IsType<OkObjectResult>(loginResult);
        Assert.NotNull(okLogin.Value);

        // Смена имени пользователя
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == _testEmail);
        var token = await _authServiceMock.Object.GenerateTokensAsync(user);
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        });
        httpContext.User = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = httpContext;

        var changeResult = await _controller.ChangeUsername(new ChangeUsernameDto { NewUsername = "newUniqueUsername" });
        Assert.IsType<OkObjectResult>(changeResult);

        // Refresh токен
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = "testRefreshToken",
            UserId = user.Id,
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });
        await _dbContext.SaveChangesAsync();

        var refreshResult = await _controller.Refresh(new RefreshDto { RefreshToken = "testRefreshToken" });
        Assert.IsType<OkObjectResult>(refreshResult);

        // Logout
        var logoutResult = await _controller.Logout(new RefreshDto { RefreshToken = "testRefreshToken" });
        Assert.IsType<OkObjectResult>(logoutResult);

        // Проверка токена (валидный)
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.CreateJwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256
            ),
            subject: new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            expires: DateTime.UtcNow.AddMinutes(5)
        );

        var tokenString = tokenHandler.WriteToken(jwt);

        var validateResult = _controller.ValidateToken(new TokenValidationDto { Token = tokenString });
        Assert.IsType<OkObjectResult>(validateResult);

        // Удаление пользователя
        var deleteResult = await _controller.DeleteUser(_testEmail);
        Assert.IsType<OkObjectResult>(deleteResult);
    }
}
