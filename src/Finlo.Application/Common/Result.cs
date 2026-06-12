namespace Finlo.Application.Common;

public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public object? Data { get; set; }

    public static Result SuccessResult(string message, object? data = null)
    {
        return new Result { Success = true, Message = message, Data = data };
    }

    public static Result FailureResult(string message, object? data = null)
    {
        return new Result { Success = false, Message = message, Data = data };
    }
}