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

        public Task DeleteWhitelistedGroup(string bearerToken, string groupId)
        {
            // get TraderId out of token data.
            return _groupRepository.DeleteWhitelistedGroup(2, groupId);   
        }

        public Task<List<GroupDto>> GetGroups(string bearerToken)
        {
            // requests a list of all groups connected to the whatsapp account through Whapi.com
            var result = _socialConnection.GetGroupsByUser(bearerToken);
            return result;
        }

        public Task<List<WhitelistedGroups>> GetWhitelistedGroups(int traderId)
        {
            return _groupRepository.GetWhitelistedGroups(traderId);
        }


        public Task WhitelistGroup(int traderId, string groupId, string groupName)
        {
            // get traderId from token data
            return _groupRepository.WhitelistGroup(2, groupId, groupName);
        }
    }
}
