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
        throw new NotImplementedException();
    }

    public Task<Trigger> CreateTrigger(Trigger trigger)
    {
        _db.Triggers.Add(trigger);
        _db.SaveChanges();
        return Task.FromResult(trigger);
    }
}