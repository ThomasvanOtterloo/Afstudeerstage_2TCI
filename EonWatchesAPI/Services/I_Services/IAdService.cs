using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Services.I_Services;

public interface IAdService
{
    
    public Task<IEnumerable<Ad>> GetAds();
    public Task<Ad> GetAdById(int id);
    public Task<Ad> CreateAd(Ad ad);
    
    
}
