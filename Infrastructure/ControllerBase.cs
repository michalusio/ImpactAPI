using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

[ApiController, Route("[controller]")]
public class ControllerBase<T>(ILogger<T> logger) : ControllerBase where T : ControllerBase<T>
{
    protected readonly ILogger<T> Logger = logger;
}
