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
            // Reject path traversal attempts
            if (filename.Contains("..") || Path.IsPathRooted(filename))
                return BadRequest("Invalid filename");

            // Only allow specific file types (optional but recommended)
            if (!filename.EndsWith(".jpg") && !filename.EndsWith(".jpeg") && !filename.EndsWith(".png"))
                return BadRequest("Unsupported file type");

            var sanitizedFileName = Path.GetFileName(filename); // Strips directory info
            var filePath = Path.Combine(_imageFolder, sanitizedFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Image not found");

            var contentType = "image/jpeg"; // or detect from file
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }





    }
}
