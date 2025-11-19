using System.Security.Cryptography;
using System.Text;
using System.Web;
using Gainly_Auth_API.Dtos;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
namespace Gainly_Auth_API.Service;
public  class TelegramAuthValidator :ITelegramAuthValidator
{
    private readonly string botToken;
    public TelegramAuthValidator(string _botToken)
    {
        botToken = _botToken;
    }

        public bool ValidateInitData(string initData,CancellationToken cancellationToken )
    {
        var query = HttpUtility.ParseQueryString(initData);

        string receivedHash = query["hash"];
        if (string.IsNullOrEmpty(receivedHash))
            return false;

        query.Remove("hash");

        var dataCheckString = string.Join("\n",
            query.AllKeys
                 .OrderBy(k => k)
                 .Select(k => $"{k}={query[k]}"));

        byte[] secretKey;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData")))
        {
            secretKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(botToken));
        }

        byte[] hashBytes;
        using (var hmac = new HMACSHA256(secretKey))
        {
            hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        }

        string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return computedHash == receivedHash;
    }

    public bool ValidateInitData(TelegramInitDataDto request,CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string>();

        data["auth_date"] = request.AuthDate;
        if (!string.IsNullOrEmpty(request.FirstName)) data["first_name"] = request.FirstName;
        if (!string.IsNullOrEmpty(request.Id.ToString())) data["id"] = request.Id.ToString();
        if (!string.IsNullOrEmpty(request.LastName)) data["last_name"] = request.LastName;
        if (!string.IsNullOrEmpty(request.PhotoUrl)) data["photo_url"] = request.PhotoUrl;
        if (!string.IsNullOrEmpty(request.Username)) data["username"] = request.Username;

        var sorted = data.OrderBy(kv => kv.Key);

        // Формируем check_string
        var checkString = string.Join("\n", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

        // SHA256(bot_token)
        using var sha = SHA256.Create();
        var secretKey = sha.ComputeHash(Encoding.UTF8.GetBytes(botToken));

        // HMAC-SHA256(check_string, secretKey)
        using var hmac = new HMACSHA256(secretKey);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(checkString));
        var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return computedHash == request.Hash;
    }
}
