using EonWatchesAPI.Enums;

namespace EonWatchesAPI.DbContext;

public class Trader
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string PhoneNumber { get; set; } = null!;
    //public Roles Role { get; set; } = Roles.TRADER;
    public ICollection<WhitelistedGroups> WhitelistedGroups { get; set; } = new List<WhitelistedGroups>();
    public ICollection<Ad> Ads { get; set; } = new List<Ad>();
    public string? WhapiBearerToken { get; set; }

}