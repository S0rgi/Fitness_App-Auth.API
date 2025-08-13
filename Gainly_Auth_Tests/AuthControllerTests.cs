using Xunit;
using Moq;
using Gainly_Auth_API.Controllers;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gainly_Auth.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk_WithTokens()
    {
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new AuthResult(true, null, new TokenPair("access", "refresh")));

        var result = await _controller.Register(new RegisterRequest("email@mail.com", "pass"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TokenPair>(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithTokens()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new AuthResult(true, null, new TokenPair("a", "r")));

        var result = await _controller.Login(new LoginRequest("email@mail.com", "pass"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TokenPair>(ok.Value);
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WhenValid()
    {
        _authServiceMock
            .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenPair("a", "r"));

        var result = await _controller.Refresh(new Gainly_Auth_API.Dtos.RefreshDto { RefreshToken = "r" });
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        _authServiceMock
            .Setup(s => s.LogoutAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Logout("r");
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Validate_ReturnsOk_WhenValid()
    {
        _authServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenValidationResult(true, null));

        var result = await _controller.Validate(new Gainly_Auth_API.Dtos.TokenValidationDto { Token = "t" });
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SendEmailCode_ReturnsOk_WithCode()
    {
        _authServiceMock
            .Setup(s => s.SendEmailCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(new EmailCodeResult(true, 12345));

        var result = await _controller.SendEmailCode("email@mail.com");
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(12345, ok.Value);
    }
}


