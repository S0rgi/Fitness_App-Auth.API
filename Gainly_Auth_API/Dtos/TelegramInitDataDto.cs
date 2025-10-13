using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Gainly_Auth_API.Dtos
{
    public record TelegramInitDataRawDto
    {
        public string initDataRaw { get; set; }
    }
    public record TelegramInitDataDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; } = string.Empty;

        [JsonPropertyName("auth_date")]
        public string AuthDate { get; set; } = string.Empty;

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
    }
}