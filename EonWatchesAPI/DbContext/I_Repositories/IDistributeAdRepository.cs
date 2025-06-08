using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.DbContext.I_Repositories;

public interface IDistributeAdRepository
{
    public Task<bool> DistributeAd(Ad ad);
    
}