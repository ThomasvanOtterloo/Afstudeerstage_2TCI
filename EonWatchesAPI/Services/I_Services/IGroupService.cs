using EonWatchesAPI.Dtos;
using RestSharp;

namespace EonWatchesAPI.Services.I_Services
{
    public interface IGroupService
    {

        public Task<List<GroupDto>> GetGroups(string bearerToken);

    }
}
