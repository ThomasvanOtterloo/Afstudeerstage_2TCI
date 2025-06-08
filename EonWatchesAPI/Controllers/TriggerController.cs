using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.Notifications;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TriggerController : ControllerBase
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

    [HttpGet("/TriggerList/{userId}")]
    public async Task<List<Trigger>> GetTriggerListByUserId(int userId)
    {
        return await _triggerService.GetTriggerListByUserId(userId);
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

    // DELETE /Trigger/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrigger(int id)
    {
        try
        {
            // Let the service return a bool indicating whether it actually deleted something
            var deleted = await _triggerService.DeleteTrigger(id);
            if (!deleted)
            {
                // 404 if that ID wasn't in the database
                return NotFound(new { message = $"No trigger found with ID {id}." });
            }

            // 204 No Content is standard for a successful DELETE
            return NoContent();
        }
        catch (Exception ex)
        {
            // Return a generic 500 with no sensitive exception details
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }






}