using EonWatchesAPI.DbContext;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TraderController : ControllerBase
{
    private readonly ITraderService _traderService;
    
    public TraderController(ITraderService traderService)
    {
        _traderService = traderService;
    }
    
    [HttpGet]
    public async Task<IEnumerable<Trader>> GetTraders()
    {
        return await _traderService.GetTraders();
    }
    
    [HttpGet("{id}")]
    public async Task<Trader> GetTraderById(int id)
    {
        return await _traderService.GetTraderById(id);
    }
    
    [HttpPost]
    public async Task<Trader> CreateTrader(Trader trader)
    {
        return await _traderService.CreateTrader(trader);
    }
    
    
}