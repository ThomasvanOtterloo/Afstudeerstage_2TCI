using EonWatchesAPI.Domain;
using EonWatchesAPI.Enums;

namespace EonWatchesAPI.DbContext;

public class Trader
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public List<Ad> Ads { get; set; } = new List<Ad>();
    public List<TraderWhitelistedGroup> TraderWhitelistedGroups { get; set; } = new List<TraderWhitelistedGroup>();
    public string? WhapiBearerToken { get; set; }
}