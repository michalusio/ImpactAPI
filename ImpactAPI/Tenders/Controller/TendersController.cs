using ImpactAPI.Tenders.Service;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ImpactAPI.Tenders.Controller;

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
