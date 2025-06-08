using EonWatchesAPI.Dtos;
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Distribute([FromForm] DistributeAdDto dto)
    {
        try
        {
            if (dto.AdEntities?.Image != null)
            {
                await _distributeAdService.SendImageToGroup(dto);
            }
            else
            {
                await _distributeAdService.SendMessageToGroup(dto);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
