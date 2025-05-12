using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class SendImageCaptionDto
    {
        [FromForm]
        [Required]
        public string BearerToken { get; set; }
        [FromForm]
        public string? Text { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "GroupIds must contain at least one group.")]
        [FromForm]
        public List<string> GroupIds { get; set; }
        
        [Required]
        [FromForm]
        public IFormFile Image { get; set; }

    }
}