using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Services.I_Services;

public interface IDistributeAdService
{
    public Task SendImageToGroup(ConnectionType[] type, string bearerToken, string caption, string base64Image, List<string> groupId);
    public Task SendMessageToGroup(ConnectionType[] type, string bearerToken, string text, List<string> groupId);


}