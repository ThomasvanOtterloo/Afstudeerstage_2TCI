using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class SendMessageDto
    {
        public string BearerToken { get; set; }
        public string Text { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "GroupIds must contain at least one group.")]
        public List<string> GroupIds { get; set; }
    }
}