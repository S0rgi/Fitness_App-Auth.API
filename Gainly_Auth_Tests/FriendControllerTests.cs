using System.Security.Claims;
using Gainly_Auth_API.Controllers;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading;

namespace Gainly_Auth.Tests;

public class FriendControllerTests
{
	private static FriendController CreateControllerWithUser(Mock<IFriendshipService> serviceMock, Guid userId)
	{
		var controller = new FriendController(serviceMock.Object);
		var httpContext = new DefaultHttpContext();
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
		httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
		controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
		return controller;
	}

	[Fact]
	public async Task SendFriendRequestByUsername_ReturnsOk_WhenCreated()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service
			.Setup(s => s.SendFriendRequestByUsernameAsync(userId, "friend", It.IsAny<CancellationToken>()))
			.ReturnsAsync((new Gainly_Auth_API.Models.Friendship(), new Gainly_Auth_API.Models.User { Username = "me" }, new Gainly_Auth_API.Models.User { Email = "f@mail.com" }));

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.SendFriendRequestByUsername("friend");
		Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task SendFriendRequestByUsername_ReturnsBadRequest_WhenInvalid()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service
			.Setup(s => s.SendFriendRequestByUsernameAsync(userId, "friend", It.IsAny<CancellationToken>()))
			.ReturnsAsync(((Gainly_Auth_API.Models.Friendship, Gainly_Auth_API.Models.User, Gainly_Auth_API.Models.User)?)null);

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.SendFriendRequestByUsername("friend");
		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task RespondToFriendRequest_ReturnsOk_WhenHandled(bool accept)
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service
			.Setup(s => s.RespondToFriendRequestAsync(It.IsAny<Guid>(), userId, accept, It.IsAny<CancellationToken>()))
			.ReturnsAsync((new Gainly_Auth_API.Models.Friendship(), new Gainly_Auth_API.Models.User(), new Gainly_Auth_API.Models.User()));

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.RespondToFriendRequest(Guid.NewGuid(), accept);
		Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task RespondToFriendRequest_ReturnsNotFound_WhenMissing()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service
			.Setup(s => s.RespondToFriendRequestAsync(It.IsAny<Guid>(), userId, true, It.IsAny<CancellationToken>()))
			.ReturnsAsync(((Gainly_Auth_API.Models.Friendship, Gainly_Auth_API.Models.User, Gainly_Auth_API.Models.User)?)null);

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.RespondToFriendRequest(Guid.NewGuid(), true);
		Assert.IsType<NotFoundObjectResult>(result);
	}

	[Fact]
	public async Task GetPendingRequests_ReturnsOk_WithList()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service.Setup(s => s.GetPendingRequestsAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<object> { new { Id = Guid.NewGuid() } });

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.GetPendingRequests();
		var ok = Assert.IsType<OkObjectResult>(result);
		Assert.NotNull(ok.Value);
	}

	[Fact]
	public async Task GetFriends_ReturnsOk_WithList()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service.Setup(s => s.GetFriendsAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<object> { new { Id = Guid.NewGuid(), Username = "f" } });

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.GetFriends();
		var ok = Assert.IsType<OkObjectResult>(result);
		Assert.NotNull(ok.Value);
	}

	[Fact]
	public async Task RemoveFriend_ReturnsOk_WhenRemoved()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service.Setup(s => s.RemoveFriendAsync(userId, "friend", It.IsAny<CancellationToken>())).ReturnsAsync(true);

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.RemoveFriend("friend");
		Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task RemoveFriend_ReturnsBadRequest_WhenNotFriends()
	{
		var service = new Mock<IFriendshipService>();
		var userId = Guid.NewGuid();
		service.Setup(s => s.RemoveFriendAsync(userId, "friend", It.IsAny<CancellationToken>())).ReturnsAsync(false);

		var controller = CreateControllerWithUser(service, userId);
		var result = await controller.RemoveFriend("friend");
		Assert.IsType<BadRequestObjectResult>(result);
	}
}


