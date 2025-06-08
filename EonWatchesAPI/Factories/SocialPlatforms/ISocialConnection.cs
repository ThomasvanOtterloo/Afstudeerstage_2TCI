using EonWatchesAPI.Dtos;

namespace EonWatchesAPI.Factories;

public interface ISocialConnection
{
    Task<string> SendTextToGroup(string bearerToken, string text, string groupIds);
    Task<string> SendImageToGroup(string bearerToken, string caption, string base64Image, string groupIds);
    Task<List<GroupDto>> GetGroupsByUser(string bearerToken);

}
