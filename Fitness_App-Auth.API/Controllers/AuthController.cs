using Microsoft.AspNetCore.Mvc;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Dtos;

namespace Fitness_App_Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return result.Success ? Ok(result.Tokens) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result.Tokens) : Unauthorized(result.ErrorMessage);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
        {
            var tokens = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return tokens != null ? Ok(tokens) : Unauthorized("Invalid refresh token");
        }

        [HttpDelete("logout")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _authService.LogoutAsync(refreshToken);
            return NoContent();
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] TokenValidationDto dto)
        {
            var result = await _authService.ValidateTokenAsync(dto.Token);
            return result.IsValid ? Ok() : Unauthorized(result.Reason);
        }

        [HttpGet("email_code/{email}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendEmailCode(string email)
        {

            EmailCodeResult res = await _authService.SendEmailCodeAsync(email);
            if (res.IsValid)
                return Ok(res.code);
            return BadRequest("Email уже занят");
        }
    }
}
