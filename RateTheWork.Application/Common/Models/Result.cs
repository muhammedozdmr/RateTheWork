namespace RateTheWork.Application.Common.Models;

/// <summary>
/// Generic result wrapper for application operations
/// </summary>
public class Result<T>
{
    private Result
    (
        bool isSuccess
        , T? data
        , string? error
        , List<string>? errors = null
        , Dictionary<string, object>? metadata = null
    )
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors ?? new List<string>();
        Metadata = metadata;
    }

    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public List<string> Errors { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static Result<T> Success(T data, Dictionary<string, object>? metadata = null)
    {
        return new Result<T>(true, data, null, null, metadata);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, default, null, errors);
    }

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess && Data != null
            ? Result<TNew>.Success(mapper(Data), Metadata)
            : Result<TNew>.Failure(Error ?? string.Join(", ", Errors));
    }
}

/// <summary>
/// Result without data
/// </summary>
public class Result
{
    private Result
        (bool isSuccess, string? error, List<string>? errors = null, Dictionary<string, object>? metadata = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
        Metadata = metadata;
    }

    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> Errors { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static Result Success(Dictionary<string, object>? metadata = null)
    {
        return new Result(true, null, null, metadata);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    public static Result Failure(List<string> errors)
    {
        return new Result(false, null, errors);
    }

    public static implicit operator Result(string error)
    {
        return Failure(error);
    }
}
