using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;

namespace EonWatchesAPI.Services.I_Services;

public interface ITriggerService
{
    public Task<IEnumerable<Trigger>> GetTriggers();
    public Task<Trigger> GetTriggerById(int id);
    public Task<Trigger> CreateTrigger(TriggerCreateDto trigger);
    
    
}