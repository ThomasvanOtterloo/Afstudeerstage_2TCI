namespace EonWatchesAPI.DbContext;

public class BlacklistedTraders
{
    public int Id { get; set; }
    public Trader Trader { get; set; }
    public Trader BlacklistedTrader { get; set; }
    public DateTime BlacklistedDate { get; set; }
    public string? Reason { get; set; }
    
}

