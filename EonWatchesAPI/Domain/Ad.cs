namespace EonWatchesAPI.DbContext;

public class Ad
{
    public int Id { get; set; }
    public string? GroupId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ReferenceNumber { get; set; }
    public Decimal? Price { get; set; }
    public string? Currency { get; set; }
    
    
    
    public bool IsAnSeller { get; set; }
    public bool Archived { get; set; }
    public DateTime CreatedAt { get; set; }
}