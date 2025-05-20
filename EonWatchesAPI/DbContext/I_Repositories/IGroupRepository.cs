namespace EonWatchesAPI.DbContext.I_Repositories
{
    public interface IGroupRepository
    {
        public Task WhitelistGroup(int traderId, string groupId, string groupName);
        public Task DeleteWhitelistedGroup(int traderId, string groupId);
        public Task<List<WhitelistedGroups>> GetWhitelistedGroups(int traderId);

    }
}
