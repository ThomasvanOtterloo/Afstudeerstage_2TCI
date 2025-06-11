namespace EonWatchesAPI.Dtos
{
    public class TraderDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? token { get; set; }
    }
}
