using Azure.Core;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.Factories;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Diagnostics.Eventing.Reader;
using static System.Net.WebRequestMethods;

namespace EonWatchesAPI.Services.Services;

public class DistributeAdService : IDistributeAdService
{
    private readonly Dictionary<ConnectionType, ISocialConnection> _strategies;

    public DistributeAdService(Dictionary<ConnectionType, ISocialConnection> strategies)
    {
        _strategies = strategies;
    }

    public async Task SendMessageToGroup(SendMessageDto ad)
    {
        foreach (var type in ad.ConnectionType)
        {
            if (!_strategies.TryGetValue(type, out var strategy))
                throw new NotSupportedException($"Unsupported connection type: {type}");

            await strategy.SendTextToGroups(ad.BearerToken, ad.Text, ad.GroupIds);
        }
    }

    public async Task SendImageToGroup(SendImageCaptionDto ad)
    {
        using var ms = new MemoryStream();
        await ad.Image.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var b64 = Convert.ToBase64String(bytes);

        var mimeType = ad.Image.ContentType; // Optional
        var dataUri = $"data:{mimeType};base64,{b64}";


        foreach (var type in ad.ConnectionType)
        {
            if (!_strategies.TryGetValue(type, out var strategy))
                throw new NotSupportedException($"Unsupported connection type: {type}");

            await strategy.SendImageToGroups(ad.BearerToken, ad.Text, dataUri, ad.GroupIds);
        }
    }

    
}

