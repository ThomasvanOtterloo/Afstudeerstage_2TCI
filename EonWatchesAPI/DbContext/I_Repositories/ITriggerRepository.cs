namespace EonWatchesAPI.DbContext.I_Repositories;

public interface ITriggerRepository
{
    public Task<IEnumerable<Trigger>> GetTriggers();
    public Task<Trigger> GetTriggerById(int id);
    public Task<List<Trigger>> GetTriggerListByUserId(int id);
    public Task<Trigger> CreateTrigger(Trigger trigger);
    public Task<bool> DeleteTrigger(int id);
    
}