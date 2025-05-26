using EonWatchesAPI.Dtos;

namespace EonWatchesAPI.Factories;

public interface ISocialConnection
{
    Task SendTextToGroups(string bearerToken, string text, List<string> groupIds);
    Task SendImageToGroups(string bearerToken, string caption, string base64Image, List<string> groupIds);
    Task<List<GroupDto>> GetGroupsByUser(string bearerToken);

}
