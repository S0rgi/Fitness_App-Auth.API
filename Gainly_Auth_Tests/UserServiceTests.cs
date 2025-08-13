using Gainly_Auth_API.Data;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Service;
using Microsoft.EntityFrameworkCore;

namespace Gainly_Auth.Tests;

public class UserServiceTests
{
	private static AuthDbContext CreateDb()
	{
		var options = new DbContextOptionsBuilder<AuthDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		return new AuthDbContext(options);
	}

	[Fact]
	public async Task ChangeUsername_ReturnsSuccess_WhenAvailable()
	{
		var db = CreateDb();
		var user = new User { Id = Guid.NewGuid(), Email = "u@mail.com", PasswordHash = "hash", Username = "old" };
		db.Users.Add(user);
		await db.SaveChangesAsync();
		var service = new UserService(db);
		var res = await service.ChangeUsernameAsync(user.Id, "new");
		Assert.Equal(Gainly_Auth_API.Interfaces.ChangeUsernameResult.Success, res);
	}

	[Fact]
	public async Task DeleteUserByEmail_ReturnsFalse_WhenMissing()
	{
		var db = CreateDb();
		var service = new UserService(db);
		var res = await service.DeleteUserByEmailAsync("e@mail.com");
		Assert.False(res);
	}

	[Fact]
	public async Task UserExistsAsync_ReturnsTrue_WhenExists()
	{
		var db = CreateDb();
		db.Users.Add(new User { Id = Guid.NewGuid(), Email = "e@mail.com", PasswordHash = "hash", Username = "u" });
		await db.SaveChangesAsync();
		var service = new UserService(db);
		var res = await service.UserExistsAsync("e@mail.com");
		Assert.True(res);
	}
}


