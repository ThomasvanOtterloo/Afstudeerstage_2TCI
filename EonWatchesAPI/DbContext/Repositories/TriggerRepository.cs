using EonWatchesAPI.DbContext.I_Repositories;
using Microsoft.EntityFrameworkCore;

namespace EonWatchesAPI.DbContext;

public class TriggerRepository : ITriggerRepository
{
    private readonly AppDbContext _db;
    
    public TriggerRepository(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<IEnumerable<Trigger>> GetTriggers()
    {
        return await _db.Triggers.ToListAsync();
    }

    public Task<Trigger> GetTriggerById(int id)
    {
        return _db.Triggers.ElementAtAsync(id);
    }

    public async Task<List<Trigger>> GetTriggerListByUserId(int id)
    {
        return await _db.Triggers
                        .Where(t => t.TraderId == id)
                        .ToListAsync();
    }

    public Task<Trigger> CreateTrigger(Trigger trigger)
    {
        _db.Triggers.Add(trigger);
        _db.SaveChanges();
        return Task.FromResult(trigger);
    }

    public async Task<bool> DeleteTrigger(int id)
    {
        var trigger = await _db.Triggers.FindAsync(id);
        if (trigger == null) return false;

        _db.Triggers.Remove(trigger);
        await _db.SaveChangesAsync();
        return true;


    }
}