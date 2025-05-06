namespace EonWatchesAPI.DbContext;

public class Ad
{
    public int Id { get; set; }
    public string MessageId { get; set; }
    public WhitelistedGroups Group { get; set; }
    public string GroupId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ReferenceNumber { get; set; }
    public Decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Image { get; set; }
    public string? Video { get; set; }
    public int TraderId { get; set; } 
    public Trader Trader { get; set; } = null!;
    public string? Color { get; set; }
    public string? Condition { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? BatchCode { get; set; }
    public string? Location { get; set; }
    public bool? FullSet { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Movement {  get; set; }
    public string? Caliber { get; set; }
    public int? CaseDiameter { get; set; }
    public string? Other {  get; set; }
    public bool? Shipping {  get; set; }
    public string? PhoneNumber { get; set; }
    public string? TraderName { get; set; }
    
    
    public bool? IsAnSeller { get; set; }
    public bool Archived { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}