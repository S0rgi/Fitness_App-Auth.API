using Fitness_App_Auth.API.Models;
using System.Security.Claims;

namespace Fitness_App_Auth.API.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        Task<AuthResult> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Аутентифицирует пользователя и возвращает токены.
        /// </summary>
        Task<AuthResult> LoginAsync(LoginRequest request);

        /// <summary>
        /// Обновляет access-токен по refresh-токену.
        /// </summary>
        Task<TokenPair> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Отзывает refresh-токен (выход из системы).
        /// </summary>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Проверяет валидность токена.
        /// </summary>
        Task<TokenValidationResult> ValidateTokenAsync(string token);

        /// <summary>
        /// Отправляет код подтверждения на e-mail.
        /// </summary>
        Task<EmailCodeResult> SendEmailCodeAsync(string email);
    }

    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record TokenPair(string AccessToken, string RefreshToken);
    public record AuthResult(bool Success, string? ErrorMessage, TokenPair? Tokens);
    public record TokenValidationResult(bool IsValid, string? Reason);
    public record EmailCodeResult(bool IsValid, int? code);
}
