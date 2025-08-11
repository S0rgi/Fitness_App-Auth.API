using Fitness_App_Auth.API.Data;
using Fitness_App_Auth.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fitness_App_Auth.API.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Service;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
namespace Fitness_App_Auth.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;

        private readonly ITokenService _tokenService;
        private readonly IUserAuthenticationService _authService;
        IUsernameGenerator _usernameGenerator;
        public UserController(AuthDbContext context, IConfiguration config, ITokenService tokenService, IUserAuthenticationService authService,IUsernameGenerator usernameGenerator)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
            _authService = authService;
            _usernameGenerator = usernameGenerator;
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

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
                return Unauthorized();

            // Проверка, занят ли ник
            var exists = await _context.Users.AnyAsync(u => u.Username == dto.NewUsername);
            if (exists)
                return BadRequest("Username занят");

            user.Username = dto.NewUsername;
            await _context.SaveChangesAsync();

            return Ok("Username изменён");
        }
        
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string email){
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user ==null)
                return BadRequest("email не найден");
            _context.Users.Remove(user);
             await _context.SaveChangesAsync();
            return Ok("user успешно удалён");
        }
        [HttpGet("UserExist")]
        public async Task<IActionResult> UserExist(string email){
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user ==null)
                return BadRequest("email не найден");
            else
                return Ok("email найден");
        }

    }
    
    }