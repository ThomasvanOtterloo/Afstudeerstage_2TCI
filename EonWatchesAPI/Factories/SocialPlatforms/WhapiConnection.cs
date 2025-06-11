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

        RestResponse response;
        try
        {
            response = await client.ExecuteAsync(request, Method.Post);
        }
        catch (Exception netEx)
        {
            // network / protocol errors (DNS, TLS, socket, etc.)
            throw new HttpRequestException(
                "Failed to connect to WhatsApp text API",
                netEx
            );
        }

        if (!response.IsSuccessful)
        {
            // capture body + any RestSharp‐side exception
            var body = response.Content ?? "<no response body>";
            var err = response.ErrorException;

            throw new HttpRequestException(
                $"WhatsApp text API returned {(int)response.StatusCode} {response.StatusDescription}: {body}",
                err
            );
        }

        try
        {
            using var doc = JsonDocument.Parse(response.Content!);
            return doc.RootElement
                      .GetProperty("message")
                      .GetProperty("id")
                      .GetString()!;
        }
        catch (Exception parseEx)
        {
            throw new InvalidOperationException(
                "Failed to parse WhatsApp text API response",
                parseEx
            );
        }
    }

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

        RestResponse response;
        try
        {
            response = await client.ExecuteAsync(request, Method.Post);
        }
        catch (Exception netEx)
        {
            throw new HttpRequestException(
                "Failed to connect to WhatsApp image API",
                netEx
            );
        }

        if (!response.IsSuccessful)
        {
            var body = response.Content ?? "<no response body>";
            var err = response.ErrorException;

            throw new HttpRequestException(
                $"WhatsApp image API returned {(int)response.StatusCode} {response.StatusDescription}: {body}",
                err
            );
        }

        try
        {
            using var doc = JsonDocument.Parse(response.Content!);
            return doc.RootElement
                      .GetProperty("message")
                      .GetProperty("id")
                      .GetString()!;
        }
        catch (Exception parseEx)
        {
            throw new InvalidOperationException(
                "Failed to parse WhatsApp image API response",
                parseEx
            );
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
