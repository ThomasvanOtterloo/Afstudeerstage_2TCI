using Azure.Core;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Diagnostics.Eventing.Reader;
using static System.Net.WebRequestMethods;

namespace EonWatchesAPI.Services.Services;

public class DistributeAdService : IDistributeAdService
{
    private readonly IDistributeAdRepository _distributeAdRepository;
    private readonly string textUrl = "https://gate.whapi.cloud/messages/text";
    private readonly string imageUrl = "https://gate.whapi.cloud/messages/image";
    public DistributeAdService(IDistributeAdRepository distributeAdRepository)
    {
        _distributeAdRepository = distributeAdRepository;
    }

    public async Task SendMessageToGroup(string bearerToken, string text, List<string> groupId)
    {
        var options = new RestClientOptions(textUrl);
        var client = new RestClient(options);
        
        foreach (var id in groupId)
        {
            var request = new RestRequest();

            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {bearerToken}");
            request.AddJsonBody(new
            {
                typing_time =3,
                to = id,
                body = text
            });

            var response = await client.PostAsync(request);
            Console.WriteLine(response.Content);
            await Task.Delay(5000);

        }
    }

    public async Task SendImageToGroup(string bearerToken, string caption, string base64Image, List<string> groupId)
    {
        var options = new RestClientOptions(imageUrl);
        var client = new RestClient(options);
        
        foreach (var id in groupId)
        {
            var request = new RestRequest();
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {bearerToken}");
            request.AddJsonBody(new
            {
                typing_time =3,
                to = id,
                caption = caption,
                media = base64Image
            });


            var response = await client.PostAsync(request);
            // Instead of letting RestSharp throw, inspect the status yourself:
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException(
                  $"Server returned {(int)response.StatusCode} {response.StatusDescription}");
            }
 

            await Task.Delay(5000);
        }
    }

}