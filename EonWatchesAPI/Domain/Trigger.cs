namespace EonWatchesAPI.DbContext;

public class Trigger
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public required Trader Trader { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ReferenceNumber { get; set; }
    public bool? Notified { get; set; } = false;
}