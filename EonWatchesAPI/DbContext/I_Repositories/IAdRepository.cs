namespace EonWatchesAPI.DbContext.I_Repositories;

public interface IAdRepository
{
    public Task<IEnumerable<Ad>> GetAds();
    public Task<Ad> GetAdById(int id);
    public Task<Ad> CreateAd(Ad ad);
}