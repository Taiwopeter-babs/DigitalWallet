namespace Storage.Response;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }

    public StatusCode StatusCode { get; init; }
    public required string Message { get; set; }
}

public static class ApiResult<T> where T : class
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

    public static ApiResponse<T> NotFound(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.NotFound,
            Message = message,
            Success = false
        };
    }

    public static ApiResponse<T> BadRequest(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.BadRequest,
            Message = message,
            Success = false
        };
    }

    public static ApiResponse<T> ServerError(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCode.ServerError,
            Message = message,
            Success = false
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