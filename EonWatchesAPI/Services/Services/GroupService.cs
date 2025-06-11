using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories;
using EonWatchesAPI.Services.I_Services;
using RestSharp;
using System.Text.Json;

namespace EonWatchesAPI.Services.Services
{


    public class GroupService : IGroupService
    {
        ISocialConnection _socialConnection;
        IGroupRepository _groupRepository;

        public GroupService(ISocialConnection socialConnection, IGroupRepository groupRepository) {
            _socialConnection = socialConnection;
            _groupRepository = groupRepository;
        
        }

        public Task DeleteWhitelistedGroup(int traderId, string groupId)
        {
            // get TraderId out of token data.
            return _groupRepository.DeleteWhitelistedGroup(traderId, groupId);   
        }

        public Task<List<GroupDto>> GetGroups(string bearerToken)
        {
            var result = _socialConnection.GetGroupsByUser(bearerToken);
            return result;
        }

        public Task<List<WhitelistedGroup>> GetWhitelistedGroups(int traderId)
        {
            return _groupRepository.GetWhitelistedGroups(traderId);
        }


        public Task WhitelistGroup(int traderId, string groupId, string groupName)
        {
            // get traderId from token data
            return _groupRepository.WhitelistGroup(traderId, groupId, groupName);
        }
    }
}
