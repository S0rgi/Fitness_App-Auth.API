namespace Gainly_Auth_API.Interfaces
{

    public interface ITelegramAuthValidator
    {
        public bool ValidateInitData(string initData, CancellationToken cancellationToken);
    }
}