using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AdController : ControllerBase
{
    private readonly IAdService _adService;

    public AdController(IAdService adService)
    {
        _adService = adService;
    }

    [HttpGet("{id}")]
    public async Task<Ad> GetAdById(int id)
    {
        return await _adService.GetAdById(id);
    }

    [HttpPost]
    public async Task<Ad> CreateAd(CreateAdDto ad)
    {
        return await _adService.CreateAd(ad);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ad>>> GetAds([FromQuery] AdFilterDto? filter)
    {
        try
        {
            var results = await _adService.GetAdsByFilter(filter);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            // returns a 400, with your ex.Message in the response body
            return BadRequest(ex.Message);
        }
    }

}