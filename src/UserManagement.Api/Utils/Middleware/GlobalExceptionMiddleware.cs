using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

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
        // TODO: Translate MongoDB exceptions in the infrastructure layer.
        catch (MongoWriteException ex) when (
            ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(ex, "Duplicate email");
            await WriteProblem(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                "A user with this email already exists.");
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Unauthorized request");
            await WriteProblem(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
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
