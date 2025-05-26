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
            // Strip out path traversal
            if (string.IsNullOrWhiteSpace(filename) || filename.Contains("..") || Path.IsPathRooted(filename))
                return BadRequest("Invalid filename");

            // Optional: Restrict file extensions
            if (!filename.EndsWith(".jpg") && !filename.EndsWith(".jpeg") && !filename.EndsWith(".png"))
                return BadRequest("Unsupported file type");

            // Combine and resolve path
            var filePath = Path.GetFullPath(Path.Combine(_imageFolder, filename));

            // Final safety check
            if (!filePath.StartsWith(Path.GetFullPath(_imageFolder)))
                return BadRequest("File access outside of permitted folder");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Image not found");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var contentType = "image/jpeg";
            return File(fileStream, contentType, enableRangeProcessing: true);
        }






    }
}
