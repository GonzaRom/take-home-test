namespace Fundo.Application.Common;

public sealed class Result<T>
{
    private readonly T? value;
    private readonly string? error;

    private Result(ResultStatus status, T? value, string? error)
    {
        Status = status;
        this.value = value;
        this.error = error;
    }

    public ResultStatus Status { get; }

    public bool IsSuccess => Status == ResultStatus.Success;

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("A failed result does not contain a value.");

    public string Error => IsSuccess
        ? throw new InvalidOperationException("A successful result does not contain an error.")
        : error ?? string.Empty;

    public static Result<T> Success(T value)
    {
        return new Result<T>(ResultStatus.Success, value, null);
    }

    public static Result<T> Invalid(string error)
    {
        return new Result<T>(ResultStatus.Invalid, default, error);
    }

    public static Result<T> NotFound(string error)
    {
        return new Result<T>(ResultStatus.NotFound, default, error);
    }
}
