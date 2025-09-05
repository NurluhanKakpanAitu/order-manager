using System.Net;
using System.Text.Json;
using Application.DTOs.Common;

namespace WebAPI.Middleware;

public class ResponseWrapperMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await next(context);
        
        context.Response.Body = originalBodyStream;
        
        if (context.Response.StatusCode is >= 200 and < 300)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            
            object apiResponse;
            if (string.IsNullOrWhiteSpace(responseText))
            {
                apiResponse = ApiResponse.Success();
            }
            else
            {
                try
                {
                    var existingData = JsonSerializer.Deserialize<JsonElement>(responseText);
                    apiResponse = ApiResponse<JsonElement>.Success(existingData);
                }
                catch
                {
                    apiResponse = ApiResponse<string>.Success(responseText);
                }
            }

            var wrappedResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(wrappedResponse);
        }
        else
        {
            // For non-success status codes, just copy the original response
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}