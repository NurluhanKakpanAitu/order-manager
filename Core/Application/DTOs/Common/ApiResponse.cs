namespace Application.DTOs.Common;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Data = data,
            IsSuccess = true,
            Error = null
        };
    }

    public static ApiResponse<T> Failure(string error)
    {
        return new ApiResponse<T>
        {
            Data = default,
            IsSuccess = false,
            Error = error
        };
    }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }

    public static ApiResponse Success()
    {
        return new ApiResponse
        {
            IsSuccess = true,
            Error = null
        };
    }

    public static ApiResponse Failure(string error)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Error = error
        };
    }
}