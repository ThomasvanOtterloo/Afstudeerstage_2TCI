using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Dtos.MappingExtensions;
using EonWatchesAPI.Services.I_Services;

namespace EonWatchesAPI.Services.Services;

public class AdService : IAdService
{
    private readonly IAdRepository _adRepository;

    public AdService(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public Task<IEnumerable<Ad>> GetAds()
    {
        // logic to get all ads

        return _adRepository.GetAds();
    }

    public Task<Ad> GetAdById(int id)
    {
        // logic to get ad by id

        return _adRepository.GetAdById(id);
    }

    public Task<Ad> CreateAd(CreateAdDto dto)
    {
        var ad = dto.ToEntity();
        return _adRepository.CreateAd(ad);
    }

    public Task<IEnumerable<Ad>> GetAdsByFilter(AdFilterDto filter)
    {
        var ads = _adRepository.GetAdsFiltered(filter.Brand, filter.Model, filter.ReferenceNumber, filter.DaysAgo);

        return ads;
    }
}