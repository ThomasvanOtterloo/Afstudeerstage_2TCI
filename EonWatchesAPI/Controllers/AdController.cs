using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdController : ControllerBase
    {
        private readonly IAdService _adService;

        public AdController(IAdService adService)
        {
            _adService = adService;
        }

        // GET /Ad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdById(int id)
        {
            var ad = await _adService.GetAdById(id);

            if (ad == null)
            {
                // No ad found → 204 No Content
                return NoContent();
            }

            // Ad found → 200 OK + payload
            return Ok(ad);
        }

        //// POST /Ad // CreateAd is integrated with DistributeAd.
        //[HttpPost]
        //public async Task<IActionResult> CreateAd([FromBody] CreateAdDto dto)
        //{
        //    // CreateAd returns an Ad object with its new Id
        //    var createdAd = await _adService.CreateAd(dto);

        //    // Return 201 Created
        //    return CreatedAtAction(
        //        nameof(GetAdById),
        //        new { id = createdAd.Id },
        //        createdAd
        //    );
        //}

        // GET /Ad?Brand=foo&…
        [HttpGet]
        public async Task<IActionResult> GetAds([FromQuery] AdFilterDto? filter)
        {
            try
            {
                var results = await _adService.GetAdsByFilter(filter);
                // 200 OK + list
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                // 400 Bad Request + exception message
                return BadRequest(ex.Message);
            }
        }
    }
}
