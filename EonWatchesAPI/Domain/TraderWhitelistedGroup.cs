using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Domain
{
    public class TraderWhitelistedGroup
    {
        public int TraderId { get; set; }
        public Trader Trader { get; set; }
        public string GroupId { get; set; }
        public WhitelistedGroup Group { get; set; }
    }
}
