using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories;
using RestSharp;
using System.Text.Json;

namespace EonWatchesAPI.Connections;

public class WhapiConnection : ISocialConnection
{
    private readonly string textUrl = "https://gate.whapi.cloud/messages/text";
    private readonly string imageUrl = "https://gate.whapi.cloud/messages/image";
    private readonly string groupUrl = "https://gate.whapi.cloud/groups?count=100";

    public async Task SendTextToGroups(string bearerToken, string text, List<string> groupIds)
    {
        var client = new RestClient(new RestClientOptions(textUrl));
        foreach (var id in groupIds)
        {
            var request = new RestRequest();
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {bearerToken}");
            request.AddJsonBody(new
            {
                typing_time = 3,
                to = id,
                body = text
            });

            var response = await client.PostAsync(request);
            Console.WriteLine(response.Content);
            await Task.Delay(3000);
        }
    }

    public async Task SendImageToGroups(string bearerToken, string caption, string base64Image, List<string> groupIds)
    {
        var client = new RestClient(new RestClientOptions(imageUrl));
        foreach (var id in groupIds)
        {
            var request = new RestRequest();
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {bearerToken}");
            request.AddJsonBody(new
            {
                typing_time = 3,
                to = id,
                caption = caption,
                media = base64Image
            });

            var response = await client.PostAsync(request);
            if (!response.IsSuccessful)
                throw new HttpRequestException($"Failed: {(int)response.StatusCode} {response.StatusDescription}");

            await Task.Delay(3000);
        }
    }

    public async Task<List<GroupDto>> GetGroupsByUser(string bearerToken)
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
                GroupName = group.GetProperty("name").GetString()
            });
        }
        return result;
    }


}
