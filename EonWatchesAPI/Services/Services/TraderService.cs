using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Dtos.MappingExtensions;
using EonWatchesAPI.Services.I_Services;

namespace EonWatchesAPI.Services.Services;

public class TraderService : ITraderService
{
    private readonly ITraderRepository _traderRepository;
    
    public TraderService(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public Task<IEnumerable<Trader>> GetTraders()
    {
        return _traderRepository.GetTraders();
    }

    public async Task<Trader> CreateTrader(TraderDto dto)
    {
        var trader = dto.ToEntity();
        return await _traderRepository.CreateTrader(trader);
    }
    
    public async Task<Trader> UpdateTrader(TraderDto dto)
    {
        var trader = dto.ToEntity();
        return await _traderRepository.UpdateTrader(trader);
    }
    
    public async Task<Trader> GetTraderById(int traderId)
    {
        return await _traderRepository.GetTraderById(traderId);
    }
    
    public async Task<IEnumerable<Trader>> GetAllTraders()
    {
        return await _traderRepository.GetAllTraders();
    }
    
    public async Task<bool> DeleteTrader(int traderId)
    {
        return await _traderRepository.DeleteTrader(traderId);
    }
    
    

}