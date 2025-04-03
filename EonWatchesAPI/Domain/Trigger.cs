namespace EonWatchesAPI.DbContext;

public class Trigger
{
    public int Id { get; set; }
    public Trader TraderId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ReferenceNumber { get; set; }
}