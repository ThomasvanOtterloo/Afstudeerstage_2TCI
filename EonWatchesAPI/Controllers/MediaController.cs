using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly string _imageFolder = @"D:/WatchesImages";
        public MediaController()
        {

        }

        [HttpGet("{filename}")]
        public IActionResult GetImage(string filename)
        {
            var filePath = Path.Combine(_imageFolder, filename);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Image not found");

            var contentType = "image/jpeg"; // You could add detection later

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }




    }
}
