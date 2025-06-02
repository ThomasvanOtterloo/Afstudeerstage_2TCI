using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Services.I_Services;

public interface IDistributeAdService
{
    public Task SendImageToGroup(SendImageCaptionDto ad);
    public Task SendMessageToGroup(SendMessageDto ad);


}