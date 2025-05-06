using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;

using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EonWatchesAPI.Services.Services;

public class TriggerService : ITriggerService
{
    private ITriggerRepository _triggerRepository;
    
    public TriggerService(ITriggerRepository triggerRepository)
    {
        _triggerRepository = triggerRepository;
    }
    
    public Task<IEnumerable<Trigger>> GetTriggers()
    {
        return _triggerRepository.GetTriggers();
    }

    public Task<Trigger> GetTriggerById(int id)
    {
        return _triggerRepository.GetTriggerById(id);
    }

    public Task<Trigger> CreateTrigger(TriggerCreateDto dto)
    {

        Trigger trigger = new Trigger {
            TraderId = dto.TraderId,
            Brand = dto.Brand,
            Model = dto.Model,
            ReferenceNumber = dto.ReferenceNumber,
        };


        return _triggerRepository.CreateTrigger(trigger);
    }
    
    
    
    
}