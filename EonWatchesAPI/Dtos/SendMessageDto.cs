using EonWatchesAPI.Factories.SocialPlatforms;
using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class SendMessageDto
    {
        [Required]
        public string BearerToken { get; set; }
        [Required]
        public ConnectionType[] ConnectionType { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "GroupIds must contain at least one group.")]
        public List<string> GroupIds { get; set; }
    }
}