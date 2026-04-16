using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Api.Utils.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next, 
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation Error");
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation error", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found");
            await WriteProblem(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled Exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, 
                "Internal server error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblem(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
            
        return context.Response.WriteAsJsonAsync(problem);
    }
}