using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.DbContext;

public class WhitelistedGroups
{
    [Key]
    public string Id { get; set; }
    public string? GroupName { get; set; }
    public Trader Trader { get; set; }
    public int TraderId { get; set; }

}