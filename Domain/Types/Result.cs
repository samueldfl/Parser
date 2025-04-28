namespace Domain.Types;

public sealed class Result<T>
{
    public bool IsFailure { get; private set; }
    public string? ErrorMessage { get; private set; }
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsFailure = !isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
