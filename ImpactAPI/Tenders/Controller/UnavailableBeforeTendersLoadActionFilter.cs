using ImpactAPI.Tenders.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImpactAPI.Tenders.Controller;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class UnavailableBeforeTendersLoadAttribute : Attribute
{
}

public class UnavailableBeforeTendersLoadActionFilter(ITenderDownloaderService DownloaderService) : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var hasAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<UnavailableBeforeTendersLoadAttribute>()
            .Any();

        if (hasAttribute)
        {
            var timeLeft = DownloaderService.TimeLeft;
            if (timeLeft.Ticks > 0)
            {
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Content = "The application has to load all tenders first - please wait just a moment."
                };
                context.HttpContext.Response.Headers.RetryAfter = timeLeft.TotalSeconds.ToString("F0");
            }
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
