using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using RestSharp;

namespace EonWatchesAPI.Services.I_Services
{
    public interface IGroupService
    {

        public Task<List<GroupDto>> GetGroups(string bearerToken);
        public Task WhitelistGroup(int traderId, string groupId, string groupName);
        public Task DeleteWhitelistedGroup(int bearerToken, string groupId);
        public Task<List<WhitelistedGroup>> GetWhitelistedGroups(int traderId);

    }
}
