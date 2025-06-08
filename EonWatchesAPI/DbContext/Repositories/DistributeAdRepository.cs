using EonWatchesAPI.DbContext.I_Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.DbContext;

public class DistributeAdRepository : IDistributeAdRepository
{
    private IAdRepository _adRepository;
    
    
    public DistributeAdRepository(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }
    
    public async Task<bool> DistributeAd(Ad ad)
    {
        await _adRepository.CreateAd(ad);
        return true;
    }
    
}