using Microsoft.AspNetCore.Mvc;
using Fitness_App_Auth.API.Interfaces;

namespace Fitness_App_Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return result.Success ? Ok(result.Tokens) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result.Tokens) : Unauthorized(result.ErrorMessage);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var tokens = await _authService.RefreshTokenAsync(refreshToken);
            return tokens != null ? Ok(tokens) : Unauthorized("Invalid refresh token");
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _authService.LogoutAsync(refreshToken);
            return NoContent();
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] string token)
        {
            var result = await _authService.ValidateTokenAsync(token);
            return result.IsValid ? Ok() : Unauthorized(result.Reason);
        }

        [HttpGet("email_code/{email}")]
        public async Task<IActionResult> SendEmailCode( string email)
        {

            EmailCodeResult res = await _authService.SendEmailCodeAsync(email);
            if (res.IsValid)
                return Ok(res.code);
            return BadRequest("Email уже занят");
        }
    }
}
