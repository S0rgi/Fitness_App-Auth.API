using Gainly_Auth_API.Models;
using System.Security.Claims;

namespace Gainly_Auth_API.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        Task<AuthResult> RegisterAsync(Gainly_Auth_API.Dtos.RegisterDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Аутентифицирует пользователя и возвращает токены.
        /// </summary>
        Task<AuthResult> LoginAsync(Gainly_Auth_API.Dtos.LoginDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет access-токен по refresh-токену.
        /// </summary>
        Task<TokenPair> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отзывает refresh-токен (выход из системы).
        /// </summary>
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет валидность токена.
        /// </summary>
        Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отправляет код подтверждения на e-mail.
        /// </summary>
        Task<EmailCodeResult> SendEmailCodeAsync(string email, CancellationToken cancellationToken = default);
    }

    public record TokenPair(string AccessToken, string RefreshToken);
    public record AuthResult(bool Success, string? ErrorMessage, TokenPair? Tokens);
    public record TokenValidationResult(bool IsValid, string? Reason);
    public record EmailCodeResult(bool IsValid, int? code);
}



