using Xunit;
using Moq;
using Gainly_Auth_API.Controllers;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Gainly_Auth_API.Dtos;

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
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(true, null, new TokenPair("access", "refresh")));

        var result = await _controller.Register(new RegisterDto { Email = "email@mail.com", Password = "pass" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TokenPair>(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithTokens()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(true, null, new TokenPair("a", "r")));

        var result = await _controller.Login(new LoginDto { Email = "email@mail.com", Password = "pass" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TokenPair>(ok.Value);
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WhenValid()
    {
        _authServiceMock
            .Setup(s => s.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenPair("a", "r"));

        var result = await _controller.Refresh(new RefreshDto { RefreshToken = "r" }, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        _authServiceMock
            .Setup(s => s.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        var result = await _controller.Logout(new RefreshDto { RefreshToken = "r" } , CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Validate_ReturnsOk_WhenValid()
    {
        _authServiceMock
            .Setup(s => s.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenValidationResult(true, null));

        var result = await _controller.Validate(new Gainly_Auth_API.Dtos.TokenValidationDto { Token = "t" }, CancellationToken.None);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SendEmailCode_ReturnsOk_WithCode()
    {
        _authServiceMock
            .Setup(s => s.SendEmailCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailCodeResult(true, null));

        var result = await _controller.SendEmailCode("email@mail.com", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(12345, ok.Value);
    }

    [Fact]
    public async Task GoogleLogin_ReturnsOk_WithTokens()
    {
        _authServiceMock
            .Setup(s => s.GoogleLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(true, null, new TokenPair("access_google", "refresh_google")));

        var result = await _controller.GoogleLogin(new GoogleLoginDto { GoogleIdToken = "some_id_token" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TokenPair>(ok.Value);
    }
}


