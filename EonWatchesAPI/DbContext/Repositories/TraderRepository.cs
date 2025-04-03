using EonWatchesAPI.DbContext.I_Repositories;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public class TraderRepository : ITraderRepository
{
    private readonly AppDbContext _context;
    
    public TraderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trader>> GetTraders()
    {
        return await _context.Traders.ToListAsync();
    }

    public Task<Trader> CreateTrader(Trader trader)
    {
        throw new NotImplementedException();
    }

    public Task<Trader> UpdateTrader(Trader trader)
    {
        throw new NotImplementedException();
    }

    public async Task<Trader> GetTraderById(int id)
    {
        return await _context.Traders.FirstOrDefaultAsync(t => t.Id == id);
    }

    public Task<IEnumerable<Trader>> GetAllTraders()
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteTrader(int traderId)
    {
        throw new NotImplementedException();
    }
}