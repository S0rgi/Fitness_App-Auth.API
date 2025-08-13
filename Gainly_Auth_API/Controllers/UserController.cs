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
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Получаем ID пользователя из токена
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var result = await _userService.ChangeUsernameAsync(Guid.Parse(userId), dto.NewUsername);
            return result switch
            {
                ChangeUsernameResult.Success => Ok("Username изменён"),
                ChangeUsernameResult.UserNotFound => Unauthorized(),
                ChangeUsernameResult.UsernameTaken => BadRequest("Username занят"),
                _ => StatusCode(500)
            };
        }
        
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string email){
            var deleted = await _userService.DeleteUserByEmailAsync(email);
            if (!deleted)
                return BadRequest("email не найден");
            return Ok("user успешно удалён");
        }
        [HttpGet("UserExist")]
        public async Task<IActionResult> UserExist(string email){
            var exists = await _userService.UserExistsAsync(email);
            if (!exists)
                return BadRequest("email не найден");
            return Ok("email найден");
        }

    }
    
    }


