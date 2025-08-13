using Xunit;
using Microsoft.AspNetCore.Mvc;
using Fitness_App_Auth.API.Controllers;
using Fitness_App_Auth.API.Interfaces;
using Moq;

namespace Fitness_App_Auth.Test
{
    public class ControllersTests
    {
        [Fact]
        public async Task Validate_WithInvalidToken_ReturnsUnauthorized()
        {
            var authService = new Mock<IAuthService>();
            authService
                .Setup(s => s.ValidateTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenValidationResult(false, "invalid"));

            var controller = new AuthController(authService.Object);

            var result = await controller.Validate(new Fitness_App_Auth.API.Dtos.TokenValidationDto { Token = "random-invalid-token" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
