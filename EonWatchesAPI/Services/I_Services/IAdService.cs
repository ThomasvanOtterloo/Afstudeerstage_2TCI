using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;

namespace EonWatchesAPI.Services.I_Services;

public interface IAdService
{
    
    public Task<IEnumerable<Ad>> GetAds();
    public Task<Ad> GetAdById(int id);
    public Task<Ad> CreateAd(Ad ad);
    public Task<IEnumerable<Ad>> GetAdsByFilter(AdFilterDto filter);
}
