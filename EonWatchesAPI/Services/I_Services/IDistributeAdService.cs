using EonWatchesAPI.DbContext;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Services.I_Services;

public interface IDistributeAdService
{
    public Task<IActionResult> DistributeAd(Ad ad);
    
}