using EonWatchesAPI.DbContext;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TriggerController
{
    private readonly ITriggerService _triggerService;
    
    public TriggerController(ITriggerService triggerService)
    {
        _triggerService = triggerService;
    }
    
    [HttpGet]
    public async Task<IEnumerable<Trigger>> GetTriggers()
    {
        return await _triggerService.GetTriggers();
    }
    
    [HttpGet("{id}")]
    public async Task<Trigger> GetTriggerById(int id)
    {
        return await _triggerService.GetTriggerById(id);
    }
    
    [HttpPost]
    public async Task<Trigger> CreateTrigger(Trigger trigger)
    {
        return await _triggerService.CreateTrigger(trigger);
    }
    
}