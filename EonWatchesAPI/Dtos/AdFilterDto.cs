using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.Dtos
{
    public class AdFilterDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? DaysAgo { get; set; }
    }


}
