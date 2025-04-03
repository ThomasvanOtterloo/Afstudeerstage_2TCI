namespace EonWatchesAPI.DbContext;

public class ArchivedAd
{
    public int Id { get; set; }
    public Ad Ad { get; set; }
    public Trader TraderId { get; set; }
    public DateTime ArchivedDate { get; set; }
    public string? Reason { get; set; }
}