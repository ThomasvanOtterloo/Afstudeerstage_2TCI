namespace EonWatchesAPI.DbContext.I_Repositories;

public interface ITraderRepository
{
    public Task<IEnumerable<Trader>> GetTraders();
    public Task<Trader> CreateTrader(Trader trader);
    public Task<Trader> UpdateTrader(Trader trader);
    public Task<Trader> GetTraderById(int id);
    public Task<IEnumerable<Trader>> GetAllTraders();
    public Task<bool> DeleteTrader(int traderId);
    
    
}