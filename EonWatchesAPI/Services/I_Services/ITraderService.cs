using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;

namespace EonWatchesAPI.Services.I_Services;

public interface ITraderService
{
    public Task<IEnumerable<Trader>> GetTraders();
    public Task<Trader> CreateTrader(TraderDto trader);
    public Task<Trader> UpdateTrader(TraderDto trader);
    public Task<Trader> GetTraderById(int traderId);
    public Task<IEnumerable<Trader>> GetAllTraders();
    public Task<bool> DeleteTrader(int traderId);
}