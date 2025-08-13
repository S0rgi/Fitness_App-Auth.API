using System.Security.Claims;
using Gainly_Auth_API.Controllers;
using Gainly_Auth_API.Dtos;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Gainly_Auth.Tests;

public class UserControllerTests
{
	private static UserController CreateController(Mock<IUserService> userService, Guid? userId = null)
	{
		var controller = new UserController(userService.Object);
		var httpContext = new DefaultHttpContext();
		if (userId.HasValue)
		{
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) };
			httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
		}
		controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
		return controller;
	}

	[Fact]
	public async Task ChangeUsername_ReturnsUnauthorized_WhenNoUser()
	{
		var service = new Mock<IUserService>();
		var controller = CreateController(service, null);
		var result = await controller.ChangeUsername(new ChangeUsernameDto { NewUsername = "new" });
		Assert.IsType<UnauthorizedResult>(result);
	}

	[Theory]
	[InlineData(ChangeUsernameResult.Success, typeof(OkObjectResult))]
	[InlineData(ChangeUsernameResult.UserNotFound, typeof(UnauthorizedResult))]
	[InlineData(ChangeUsernameResult.UsernameTaken, typeof(BadRequestObjectResult))]
	public async Task ChangeUsername_ResolvesProperly(ChangeUsernameResult svcResult, Type expected)
	{
		var service = new Mock<IUserService>();
		var userId = Guid.NewGuid();
		service.Setup(s => s.ChangeUsernameAsync(userId, "new"))
			.ReturnsAsync(svcResult);
		var controller = CreateController(service, userId);
		var result = await controller.ChangeUsername(new ChangeUsernameDto { NewUsername = "new" });
		Assert.IsType(expected, result);
	}

	[Fact]
	public async Task DeleteUser_ReturnsOk_WhenDeleted()
	{
		var service = new Mock<IUserService>();
		service.Setup(s => s.DeleteUserByEmailAsync("e"))
			.ReturnsAsync(true);
		var controller = CreateController(service, Guid.NewGuid());
		var result = await controller.DeleteUser("e");
		Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task DeleteUser_ReturnsBadRequest_WhenNotFound()
	{
		var service = new Mock<IUserService>();
		service.Setup(s => s.DeleteUserByEmailAsync("e"))
			.ReturnsAsync(false);
		var controller = CreateController(service, Guid.NewGuid());
		var result = await controller.DeleteUser("e");
		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Fact]
	public async Task UserExist_ReturnsOk_WhenFound()
	{
		var service = new Mock<IUserService>();
		service.Setup(s => s.UserExistsAsync("e"))
			.ReturnsAsync(true);
		var controller = CreateController(service, Guid.NewGuid());
		var result = await controller.UserExist("e");
		Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task UserExist_ReturnsBadRequest_WhenMissing()
	{
		var service = new Mock<IUserService>();
		service.Setup(s => s.UserExistsAsync("e"))
			.ReturnsAsync(false);
		var controller = CreateController(service, Guid.NewGuid());
		var result = await controller.UserExist("e");
		Assert.IsType<BadRequestObjectResult>(result);
	}
}


