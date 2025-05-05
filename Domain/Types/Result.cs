namespace Domain.Types;

public sealed class Result<T>
{
    public bool IsFailure { get; private set; }
    public string ErrorMessage { get; private set; }
    public T? Value { get; private set; }

    private Result(bool isSuccess, string errorMessage)
    {
        IsFailure = !isSuccess;
        ErrorMessage = errorMessage;
    }

    private Result(bool isSuccess, T? value, string errorMessage)
    {
        IsFailure = !isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success() => new(true, string.Empty);

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
