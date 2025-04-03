using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Factories.Notifications;

namespace EonWatchesAPI.Services.Services;

public class TriggerCheckerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationFactory _notificationFactory;
    
    public TriggerCheckerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repoTrigger = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
            var repoAds = scope.ServiceProvider.GetRequiredService<IAdRepository>();

            List<Ad> ads = repoAds.GetAds().Result.ToList();
            List<Trigger> triggers = repoTrigger.GetTriggers().Result.ToList();

            foreach (Trigger trigger in triggers)
            {
                foreach (Ad ad in ads)
                {
                    if (trigger.ReferenceNumber == ad.ReferenceNumber)
                    {
                        
                    }
                }
            }

            var modelExists = await repoTrigger.GetTriggers(); // your logic
            
            
            // gewoon vragen wat hun adviseren voor triggers...
            Console.WriteLine($"Checked triggers at {DateTime.Now}");

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
    
    
    
    
}