using System.Net;
using System.Text.Json;
using Application.DTOs.Common;

namespace WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing the request. TraceId: {TraceId}", 
                context.TraceIdentifier);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
            return;
            
        context.Response.ContentType = "application/json";
        
        var response = context.Response;
        string errorMessage;
        
        switch (exception)
        {
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorMessage = exception.Message;
                break;
            
            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorMessage = exception.Message;
                break;
            
            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorMessage = "Resource not found";
                break;
            
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorMessage = "Access denied";
                break;
            
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = "An internal server error occurred";
                break;
        }

        var apiResponse = ApiResponse<object>.Failure(errorMessage);

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}