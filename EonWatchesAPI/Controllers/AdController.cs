using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
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

    [HttpGet]
    public async Task<IEnumerable<Ad>> GetAds()
    {
        return await _adService.GetAds();
    }

    [HttpGet("{id}")]
    public async Task<Ad> GetAdById(int id)
    {
        return await _adService.GetAdById(id);
    }

    [HttpPost]
    public async Task<Ad> CreateAd(Ad ad)
    {
        return await _adService.CreateAd(ad);
    }
}