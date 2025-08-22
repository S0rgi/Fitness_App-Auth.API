using System.Security.Cryptography;
using System.Text;
using System.Web;
using Gainly_Auth_API.Dtos;
using Microsoft.AspNetCore.WebUtilities;
namespace Gainly_Auth_API.Service;
public  class TelegramAuthValidator
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

}
