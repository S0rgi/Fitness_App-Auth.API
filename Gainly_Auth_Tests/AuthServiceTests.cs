using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading;
using Gainly_Auth_API.Dtos;

namespace Gainly_Auth.Tests;

public class AuthServiceTests
{
	private static AuthService CreateService(
		Mock<IUserRepository> users,
		Mock<IRefreshTokenRepository> refreshTokens,
		Mock<INotificationPublisher> publisher,
		Mock<IUserAuthenticationService> tokenGen,
		Mock<ITokenService> tokenService,
		Mock<IUsernameGenerator> usernameGenerator,
		Mock<ITelegramAuthValidator> telegramAuthValidator)
	{
		return new AuthService( publisher.Object, tokenGen.Object, tokenService.Object, usernameGenerator.Object, users.Object, refreshTokens.Object, telegramAuthValidator.Object);
	}

	private static AuthDbContext CreateInMemoryDb()
	{
		var options = new DbContextOptionsBuilder<AuthDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		return new AuthDbContext(options);
	}

	[Fact]
	public async Task Register_CreatesUser_AndReturnsTokens()
	{
		var db = CreateInMemoryDb();
		var users = new Mock<IUserRepository>();
		var refreshTokens = new Mock<IRefreshTokenRepository>();
		var publisher = new Mock<INotificationPublisher>();
		var tokenGen = new Mock<IUserAuthenticationService>();
		var tokenService = new Mock<ITokenService>();
		var usernameGen = new Mock<IUsernameGenerator>();
		var telegramAuthValidator = new Mock<ITelegramAuthValidator>();

		users.Setup(r => r.ExistsByEmailAsync("e@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
		users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		users.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		usernameGen.Setup(g => g.GenerateAsync("e@mail.com")).ReturnsAsync("user1");
		tokenGen.Setup(t => t.GenerateTokensAsync(It.IsAny<User>()))
			.ReturnsAsync(new TokenPair("a", "r"));

		var service = CreateService( users, refreshTokens, publisher, tokenGen, tokenService, usernameGen,telegramAuthValidator);
        var result = await service.RegisterAsync(new RegisterDto { Email = "e@mail.com", Password = "pass" });

		Assert.True(result.Success);
		Assert.NotNull(result.Tokens);
	}

	[Fact]
	public async Task Login_ReturnsTokens_WhenCredentialsValid()
	{
		var db = CreateInMemoryDb();
		var users = new Mock<IUserRepository>();
		var refreshTokens = new Mock<IRefreshTokenRepository>();
		var publisher = new Mock<INotificationPublisher>();
		var tokenGen = new Mock<IUserAuthenticationService>();
		var tokenService = new Mock<ITokenService>();
		var usernameGen = new Mock<IUsernameGenerator>();
		var telegramAuthValidator = new Mock<ITelegramAuthValidator>();

		var user = new User { Id = Guid.NewGuid(), Email = "e@mail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
		users.Setup(r => r.FindByEmailAsync("e@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
		tokenGen.Setup(t => t.GenerateTokensAsync(user)).ReturnsAsync(new TokenPair("a", "r"));

		var service = CreateService( users, refreshTokens, publisher, tokenGen, tokenService, usernameGen,telegramAuthValidator);
        var result = await service.LoginAsync(new LoginDto { Email = "e@mail.com", Password = "pass" });

		Assert.True(result.Success);
		Assert.NotNull(result.Tokens);
	}

	[Fact]
	public async Task RefreshToken_ReturnsNull_WhenInvalid()
	{
		var db = CreateInMemoryDb();
		var users = new Mock<IUserRepository>();
		var refreshTokens = new Mock<IRefreshTokenRepository>();
		refreshTokens.Setup(r => r.FindByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);
		var publisher = new Mock<INotificationPublisher>();
		var tokenGen = new Mock<IUserAuthenticationService>();
		var tokenService = new Mock<ITokenService>();
		var usernameGen = new Mock<IUsernameGenerator>();
		var telegramAuthValidator = new Mock<ITelegramAuthValidator>();

		var service = CreateService( users, refreshTokens, publisher, tokenGen, tokenService, usernameGen,telegramAuthValidator);
		var tokens = await service.RefreshTokenAsync("bad");
		Assert.Null(tokens);
	}

	[Fact]
	public async Task ValidateToken_ReturnsValid_WhenServiceAccepts()
	{
		var db = CreateInMemoryDb();
		var users = new Mock<IUserRepository>();
		var refreshTokens = new Mock<IRefreshTokenRepository>();
		var publisher = new Mock<INotificationPublisher>();
		var tokenGen = new Mock<IUserAuthenticationService>();
		var tokenService = new Mock<ITokenService>();
		tokenService.Setup(s => s.ValidateAccessToken("t")).Returns(new System.Security.Claims.ClaimsPrincipal());
		var usernameGen = new Mock<IUsernameGenerator>();
		var telegramAuthValidator = new Mock<ITelegramAuthValidator>();

		var service = CreateService( users, refreshTokens, publisher, tokenGen, tokenService, usernameGen,telegramAuthValidator);
		var validate = await service.ValidateTokenAsync("t");
		Assert.True(validate.IsValid);
	}

	[Fact]
	public async Task SendEmailCode_ReturnsInvalid_WhenEmailExists()
	{
		var db = CreateInMemoryDb();
		var users = new Mock<IUserRepository>();
		users.Setup(r => r.ExistsByEmailAsync("e@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
		var refreshTokens = new Mock<IRefreshTokenRepository>();
		var publisher = new Mock<INotificationPublisher>();
		var tokenGen = new Mock<IUserAuthenticationService>();
		var tokenService = new Mock<ITokenService>();
		var usernameGen = new Mock<IUsernameGenerator>();
		var telegramAuthValidator = new Mock<ITelegramAuthValidator>();

		var service = CreateService( users, refreshTokens, publisher, tokenGen, tokenService, usernameGen,telegramAuthValidator);
		var res = await service.SendEmailCodeAsync("e@mail.com");
		Assert.False(res.IsValid);
	}
}


