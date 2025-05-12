using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using RestSharp;
using System.Text.Json;

namespace EonWatchesAPI.Services.Services
{


    public class GroupService : IGroupService
    {
        private readonly string groupUrl = "https://gate.whapi.cloud/groups?count=100";


        public GroupService() { }

        public async Task<List<GroupDto>> GetGroups(string bearerToken)
        {
            var options = new RestClientOptions(groupUrl);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {bearerToken}");

            var response = await client.GetAsync(request);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                return new List<GroupDto>();

            using var doc = JsonDocument.Parse(response.Content);
            var groupsElement = doc.RootElement.GetProperty("groups");

            var result = new List<GroupDto>();

            foreach (var group in groupsElement.EnumerateArray())
            {
                result.Add(new GroupDto
                {
                    Id = group.GetProperty("id").GetString(),
                    Name = group.GetProperty("name").GetString()
                });
            }
            return result;

        }
    }
}
