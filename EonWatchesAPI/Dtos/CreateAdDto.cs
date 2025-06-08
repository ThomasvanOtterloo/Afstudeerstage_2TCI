using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Dtos
{
    public class CreateAdDto
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ReferenceNumber { get; set; }
        public Decimal? Price { get; set; }
        //public string MessageId { get; set; }
        //public string? GroupId { get; set; }
        public string? Currency { get; set; }
        public IFormFile? Image { get; set; }
        public string? Video { get; set; }
        public int TraderId { get; set; }
        public string? Color { get; set; }
        public string? Condition { get; set; }
        public string? YearOfManufacture { get; set; }
        public string? BatchCode { get; set; }
        public string? Location { get; set; }
        public bool? FullSet { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Movement { get; set; }
        public string? Caliber { get; set; }
        public int? CaseDiameter { get; set; }
        public string? Other { get; set; }
        public bool? Shipping { get; set; }
    }
}
