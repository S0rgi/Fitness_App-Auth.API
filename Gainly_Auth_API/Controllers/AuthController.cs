using Microsoft.AspNetCore.Mvc;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Dtos;

namespace Gainly_Auth_API.Controllers
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _authService.RegisterAsync(request, ct);
            if (result.Success) return Ok(result.Tokens);
            return Problem(title: "Registration failed", detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _authService.LoginAsync(request, ct);
            if (result.Success) return Ok(result.Tokens);
            return Problem(title: "Unauthorized", detail: result.ErrorMessage, statusCode: StatusCodes.Status401Unauthorized);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var tokens = await _authService.RefreshTokenAsync(dto.RefreshToken, ct);
            if (tokens != null) return Ok(tokens);
            return Problem(title: "Unauthorized", detail: "Invalid refresh token", statusCode: StatusCodes.Status401Unauthorized);
        }

        [HttpDelete("logout")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Logout([FromBody] RefreshDto dto, CancellationToken ct)
        {
            var loggedOut = await _authService.LogoutAsync(dto.RefreshToken, ct);
            if (!loggedOut)
                return Problem(title: "Not Found", detail: "Refresh token not found", statusCode: StatusCodes.Status404NotFound);
            return NoContent();
        }

        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Validate([FromBody] TokenValidationDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _authService.ValidateTokenAsync(dto.Token, ct);
            if (result.IsValid) return Ok();
            return Problem(title: "Unauthorized", detail: result.Reason, statusCode: StatusCodes.Status401Unauthorized);
        }

        [HttpGet("email_code/{email}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendEmailCode(string email, CancellationToken ct)
        {
            var res = await _authService.SendEmailCodeAsync(email, ct);
            if (res.IsValid) return Ok(res.code);
            return Problem(title: "Email busy", detail: "Email уже занят", statusCode: StatusCodes.Status400BadRequest);
        }

        [HttpPost("google")]
        [ProducesResponseType(typeof(TokenPair), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _authService.GoogleLoginAsync(request.GoogleIdToken, ct);
            if (result.Success) return Ok(result.Tokens);
            
            return Problem(title: "Login Failed", detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

        }
    }
}



