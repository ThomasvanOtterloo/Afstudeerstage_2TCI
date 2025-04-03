using EonWatchesAPI.DbContext;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DistributeAdController : ControllerBase
{
    private readonly IDistributeAdService _distributeAdService;
    
    public DistributeAdController(IDistributeAdService distributeAdService)
    {
        _distributeAdService = distributeAdService;
    }
    
    [HttpPost]
    public async Task<IActionResult> DistributeAd(Ad distributeAd)
    {
        return await _distributeAdService.DistributeAd(distributeAd);
    }

}