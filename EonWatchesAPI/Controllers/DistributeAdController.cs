using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IO;


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
    
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(SendMessageDto ad)
    {
        try
        {
            await _distributeAdService.SendMessageToGroup(ad);
            return Ok();
        }
        catch (Exception ex) {
            return BadRequest(ex.Message);

        }
    }

    [HttpPost("image")]
    public async Task<IActionResult> SendMessageWithImage(SendImageCaptionDto ad)
    {
        try
        {
            await _distributeAdService.SendImageToGroup(ad);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}