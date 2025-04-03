using EonWatchesAPI.DbContext;

namespace EonWatchesAPI.Services.I_Services;

public interface ITraderService
{
    public Task<IEnumerable<Trader>> GetTraders();
    public Task<Trader> CreateTrader(Trader trader);
    public Task<Trader> UpdateTrader(Trader trader);
    public Task<Trader> GetTraderById(int traderId);
    public Task<IEnumerable<Trader>> GetAllTraders();
    public Task<bool> DeleteTrader(int traderId);
}