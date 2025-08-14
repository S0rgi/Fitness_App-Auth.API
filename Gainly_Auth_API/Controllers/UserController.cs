using Gainly_Auth_API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Gainly_Auth_API.Dtos;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace Gainly_Auth_API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        [HttpPatch("change-username")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Получаем ID пользователя из токена
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);

            if (!Guid.TryParse(userId, out var id))
                return ValidationProblem(title: "Invalid user id", statusCode: StatusCodes.Status400BadRequest);

            var result = await _userService.ChangeUsernameAsync(id, dto.NewUsername);
            return result switch
            {
                ChangeUsernameResult.Success => Ok("Username изменён"),
                ChangeUsernameResult.UserNotFound => Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized),
                ChangeUsernameResult.UsernameTaken => Problem(title: "Bad Request", detail: "Username занят", statusCode: StatusCodes.Status400BadRequest),
                _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
            };
        }
        
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string email, CancellationToken ct){
            var deleted = await _userService.DeleteUserByEmailAsync(email);
            if (!deleted)
                return Problem(title: "Bad Request", detail: "email не найден", statusCode: StatusCodes.Status400BadRequest);
            return Ok("user успешно удалён");
        }
        [HttpGet("UserExist")]
        public async Task<IActionResult> UserExist(string email, CancellationToken ct){
            var exists = await _userService.UserExistsAsync(email);
            if (!exists)
                return Problem(title: "Bad Request", detail: "email не найден", statusCode: StatusCodes.Status400BadRequest);
            return Ok("email найден");
        }

    }
    
    }


