using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.Notifications;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TriggerController
{
    private readonly ITriggerService _triggerService;
    private readonly INotification _notification;

    public TriggerController(ITriggerService triggerService, INotification notification)
    {
        _triggerService = triggerService;
        this._notification = notification;
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
    public async Task<Trigger> CreateTrigger(TriggerCreateDto trigger)
    {
        return await _triggerService.CreateTrigger(trigger);
    }
    
    [HttpPost("mail")]
    public async Task<string> CreateTrigger(SendEmailRequest mail)
    {
        try
        {
            await _notification.SendNotification(mail);
            return "Email sent successfully";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ("Error: " + e.Message + " " + e.InnerException?.Message + " " + e.StackTrace + " " + e.Source + " " + e.TargetSite);
            throw;
        }
    }
}