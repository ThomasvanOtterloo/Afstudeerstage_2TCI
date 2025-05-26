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

    public async Task SendMessageToGroup(ConnectionType[] connections, string token, string text, List<string> groupIds)
    {
        foreach (var type in connections)
        {
            if (!_strategies.TryGetValue(type, out var strategy))
                throw new NotSupportedException($"Unsupported connection type: {type}");

            await strategy.SendTextToGroups(token, text, groupIds);
        }
    }

    public async Task SendImageToGroup(ConnectionType[] connections, string token, string caption, string base64, List<string> groupIds)
    {
        foreach (var type in connections)
        {
            if (!_strategies.TryGetValue(type, out var strategy))
                throw new NotSupportedException($"Unsupported connection type: {type}");

            await strategy.SendImageToGroups(token, caption, base64, groupIds);
        }
    }
}

