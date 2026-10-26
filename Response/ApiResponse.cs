namespace Storage.Response;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }

    public StatusCode StatusCode { get; init; }
    public string? Message { get; set; }

    public Dictionary<string, string[]>? Error { get; set; }
}

public static class ApiResult<T>
{
    public static ApiResponse<T> Ok(T data, string message)
    {
        return new ApiResponse<T>
        {
            Data = data,
            StatusCode = StatusCode.Ok,
            Message = message,
            Success = true
        };
    }

    public static ApiResponse<T> NotFound(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.NotFound,
            Message = message,
            Success = false,
            Error = errors
        };
    }

    public static ApiResponse<T> BadRequest(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.BadRequest,
            Message = message,
            Success = false,
            Error = errors
        };
    }

    public static ApiResponse<T> ServerError(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.ServerError,
            Message = message,
            Success = false,
            Error = errors
        };
    }
}

public enum StatusCode
{
    Ok = 200,
    NotFound = 404,
    BadRequest = 400,
    ServerError = 500
}