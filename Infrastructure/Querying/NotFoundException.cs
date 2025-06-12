using Microsoft.AspNetCore.Http;

namespace Infrastructure.Querying;
public class NotFoundException : Exception
{
}

public class NotFoundExceptionMiddleware(RequestDelegate Next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (NotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}