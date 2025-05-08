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
            // 1) Fetch the image bytes from your localhost URL
            using var http = new HttpClient();
            byte[] bytes = await http.GetByteArrayAsync(ad.Images[0]);

            // 2) Base64-encode
            var b64 = Convert.ToBase64String(bytes);

            // 3) Build the data-URI
            var dataUri = $"data:image/jpeg;base64,{b64}";

            // 4) Pass the data-URI to your service
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
            return BadRequest(ex.Message);
        }
    }

}