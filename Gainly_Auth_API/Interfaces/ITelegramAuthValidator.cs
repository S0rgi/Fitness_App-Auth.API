using Gainly_Auth_API.Dtos;

namespace Gainly_Auth_API.Interfaces
{

    public interface ITelegramAuthValidator
    {
        public bool ValidateInitData(string initData, CancellationToken cancellationToken = default);
        public bool ValidateInitData(TelegramInitDataDto request, CancellationToken cancellationToken = default);
    }
}