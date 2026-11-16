namespace LibraryApi.Services;
 
public enum ErrorType { NotFound, Conflict }

public record Error(ErrorType Type, string Message);

public sealed class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    public Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(ErrorType type, string message) => new(default, new Error(type, message));

}
