using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{
    public record TelegramInitDataDto
    {
        public string initDataRaw { get; set; }
    }
}