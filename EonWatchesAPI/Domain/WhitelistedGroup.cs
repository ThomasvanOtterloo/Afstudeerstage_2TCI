using EonWatchesAPI.Domain;
using System.ComponentModel.DataAnnotations;

namespace EonWatchesAPI.DbContext;

public class WhitelistedGroup
{
    public string Id { get; set; }
    public string? GroupName { get; set; }
    public List<TraderWhitelistedGroup> TraderWhitelistedGroups { get; set; } = new List<TraderWhitelistedGroup>();
    public List<Ad> Ads { get; set; } = new List<Ad>();
}