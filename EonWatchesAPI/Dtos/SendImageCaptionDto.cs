using EonWatchesAPI.Factories.SocialPlatforms;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class DistributeAdDto
    {
        [FromForm]
        [Required]
        public int Token { get; set; }

        [FromForm]
        [Required]
        public ConnectionType[] ConnectionType { get; set; }

        [FromForm]
        public CreateAdDto AdEntities { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "GroupIds must contain at least one group.")]
        [FromForm]
        public List<string> GroupIds { get; set; }
        
    }
}