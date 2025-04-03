using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Services.Services;

public class DistributeAdService : IDistributeAdService
{
    private readonly IDistributeAdRepository _distributeAdRepository;
    
    public DistributeAdService(IDistributeAdRepository distributeAdRepository)
    {
        _distributeAdRepository = distributeAdRepository;
    }
    
    public Task<IActionResult> DistributeAd(Ad ad)
    {
        return _distributeAdRepository.DistributeAd(ad);
    }
}