using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Services.I_Services;

public interface IDistributeAdService
{
    public Task SendImageToGroup(string bearerToken, string caption, string base64Image, List<string> groupId);
    public Task SendMessageToGroup(string bearerToken, string text, List<string> groupId);


}