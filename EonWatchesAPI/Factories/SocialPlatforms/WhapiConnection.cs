using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories;
using RestSharp;
using System.Text.Json;

namespace EonWatchesAPI.Factories.SocialPlatforms;

public class WhapiConnection : ISocialConnection
{
    private readonly string textUrl = "https://gate.whapi.cloud/messages/text";
    private readonly string imageUrl = "https://gate.whapi.cloud/messages/image";
    private readonly string groupUrl = "https://gate.whapi.cloud/groups?count=100";

    public async Task<string> SendTextToGroup(
    string bearerToken,
    string text,
    string groupId
)
    {
        var client = new RestClient(new RestClientOptions(textUrl));
        var request = new RestRequest()
            .AddHeader("accept", "application/json")
            .AddHeader("authorization", $"Bearer {bearerToken}")
            .AddJsonBody(new
            {
                typing_time = 2,
                to = groupId,
                body = text
            });

        var response = await client.PostAsync(request);
        if (!response.IsSuccessful)
            throw new HttpRequestException(
                $"Failed: {(int)response.StatusCode} {response.StatusDescription}"
            );

        using var doc = JsonDocument.Parse(response.Content!);
        return doc.RootElement
                  .GetProperty("message")
                  .GetProperty("id")
                  .GetString()!;
    }

    // 1) Update your ISocialConnection implementation to handle one groupId and return the DTO:
    public async Task<string> SendImageToGroup(
        string bearerToken,
        string caption,
        string base64Image,
        string groupId
    )
    {
        var client = new RestClient(new RestClientOptions(imageUrl));
        var request = new RestRequest()
            .AddHeader("accept", "application/json")
            .AddHeader("authorization", $"Bearer {bearerToken}")
            .AddJsonBody(new
            {
                typing_time = 2,
                to = groupId,
                caption,
                media = base64Image
            });

        var response = await client.PostAsync(request);
        if (!response.IsSuccessful)
            throw new HttpRequestException(
                $"Failed: {(int)response.StatusCode} {response.StatusDescription}"
            );

        // parse out the message.id
        using var doc = JsonDocument.Parse(response.Content!);
        string messageId = doc.RootElement
                           .GetProperty("message")
                           .GetProperty("id")
                           .GetString()!;

        // return our DistributeAdResultDto
        return messageId;
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
