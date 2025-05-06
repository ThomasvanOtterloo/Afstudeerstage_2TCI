using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http.HttpResults;
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
    
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(SendMessageDto ad)
    {
        try
        {
            await _distributeAdService.SendMessageToGroup(ad.BearerToken, ad.Text, ad.GroupIds);
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
            await _distributeAdService.SendImageToGroup(ad.BearerToken, ad.Text, ad.Images[0], ad.GroupIds);
            return Ok();
        }
        catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }
}