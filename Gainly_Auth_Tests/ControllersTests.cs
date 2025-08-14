using Xunit;
using Microsoft.AspNetCore.Mvc;
using Gainly_Auth_API.Controllers;
using Gainly_Auth_API.Interfaces;
using Moq;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Gainly_Auth.Tests
{
    public class ControllersTests
    {
        [Fact]
        public async Task Validate_WithInvalidToken_ReturnsUnauthorized()
        {
            var authService = new Mock<IAuthService>();
            authService
                .Setup(s => s.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResult(false, "invalid"));

            var controller = new AuthController(authService.Object);

            var result = await controller.Validate(new Gainly_Auth_API.Dtos.TokenValidationDto { Token = "random-invalid-token" }, CancellationToken.None);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, obj.StatusCode);
        }
    }
}
