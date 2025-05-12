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
            using var ms = new MemoryStream();
            await ad.Image.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var b64 = Convert.ToBase64String(bytes);

            var mimeType = ad.Image.ContentType; // Optional
            var dataUri = $"data:{mimeType};base64,{b64}";

            await _distributeAdService.SendImageToGroup(
                ad.BearerToken,
                ad.Text,
                dataUri,
                ad.GroupIds
            );

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("? Error in DistributeAdController.SendMessageWithImage");
            return BadRequest(ex.Message);
        }
    }

}