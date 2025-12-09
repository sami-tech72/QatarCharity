namespace Application.Models;

public class Result<T>
{
    public bool Success { get; }
    public bool IsSuccess => Success;
    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public T? Value { get; }

    private Result(bool success, T? value, string? errorCode, string? errorMessage)
    {
        Success = success;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(true, value, null, null);

    public static Result<T> Fail(string errorCode, string errorMessage) => new(false, default, errorCode, errorMessage);
}
