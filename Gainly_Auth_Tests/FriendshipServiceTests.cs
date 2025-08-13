using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Service;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading;

namespace Gainly_Auth.Tests;

public class FriendshipServiceTests
{
	private static AuthDbContext CreateDb()
	{
		var options = new DbContextOptionsBuilder<AuthDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		return new AuthDbContext(options);
	}

	[Fact]
	public async Task SendFriendRequestByUsername_ReturnsTuple_WhenOk()
	{
		var db = CreateDb();
		var publisher = new Mock<INotificationPublisher>();
		var users = new Mock<IUserRepository>();
		var friendships = new Mock<IFriendshipRepository>();

		var senderId = Guid.NewGuid();
		var friend = new User { Id = Guid.NewGuid(), Email = "f@mail.com", Username = "f" };
		var sender = new User { Id = senderId, Username = "me" };
		users.Setup(r => r.FindByIdAsync(senderId, It.IsAny<CancellationToken>())).ReturnsAsync(sender);
		users.Setup(r => r.FindByUsernameAsync("f", It.IsAny<CancellationToken>())).ReturnsAsync(friend);
		friendships.Setup(r => r.FriendshipExistsAsync(senderId, friend.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
		friendships.Setup(r => r.AddAsync(It.IsAny<Friendship>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		friendships.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

		var service = new FriendshipService(db, publisher.Object, users.Object, friendships.Object);
		var result = await service.SendFriendRequestByUsernameAsync(senderId, "f");
		Assert.NotNull(result);
	}

	[Fact]
	public async Task RespondToFriendRequest_ReturnsNull_WhenNotForUser()
	{
		var db = CreateDb();
		var publisher = new Mock<INotificationPublisher>();
		var users = new Mock<IUserRepository>();
		var friendships = new Mock<IFriendshipRepository>();

		var friendship = new Friendship { Id = Guid.NewGuid(), Friend = new User { Username = "f" }, User = new User { Email = "u@mail.com" }, FriendId = Guid.NewGuid() };
		friendships.Setup(r => r.FindByIdAsync(friendship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(friendship);

		var service = new FriendshipService(db, publisher.Object, users.Object, friendships.Object);
		var result = await service.RespondToFriendRequestAsync(friendship.Id, Guid.NewGuid(), true);
		Assert.Null(result);
	}

	[Fact]
	public async Task GetFriends_DelegatesToRepository()
	{
		var db = CreateDb();
		var publisher = new Mock<INotificationPublisher>();
		var users = new Mock<IUserRepository>();
		var friendships = new Mock<IFriendshipRepository>();
		friendships.Setup(r => r.GetFriendsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<object> { new { Id = Guid.NewGuid() } });

		var service = new FriendshipService(db, publisher.Object, users.Object, friendships.Object);
		var result = await service.GetFriendsAsync(Guid.NewGuid());
		Assert.Single(result);
	}
}


