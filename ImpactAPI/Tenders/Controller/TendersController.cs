using ImpactAPI.Tenders.Service;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ImpactAPI.Tenders.Controller;

// Cache on the server for a whole day, cache on the client for an hour
// We set the vary to all query keys so requests for different filters are treated separately
[OutputCache(Duration = 86400, VaryByQueryKeys = ["*"])]
[ResponseCache(Duration = 3600, VaryByQueryKeys = ["*"])]
[UnavailableBeforeTendersLoad]
public class TendersController(TenderService Service, ILogger<TendersController> logger) : ControllerBase<TendersController>(logger)
{
    [HttpGet]
    public async Task<IActionResult> GetAllTenders([FromQuery] TenderQueryParameters queryParams, CancellationToken cancellationToken)
    {
        return Ok(await Service.GetTenders(queryParams, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenderById(string id, CancellationToken cancellationToken)
    {
        return Ok(await Service.GetTenderById(id, cancellationToken));
    }
}
