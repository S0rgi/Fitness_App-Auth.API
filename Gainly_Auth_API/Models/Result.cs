namespace Gainly_Auth_API.Models
{
    public readonly record struct Result(bool Success, string? Error = null)
    {
        public static Result Ok() => new(true, null);
        public static Result Fail(string error) => new(false, error);
    }

    public readonly record struct Result<T>(bool Success, T? Value, string? Error = null)
    {
        public static Result<T> Ok(T value) => new(true, value, null);
        public static Result<T> Fail(string error) => new(false, default, error);
    }
}


