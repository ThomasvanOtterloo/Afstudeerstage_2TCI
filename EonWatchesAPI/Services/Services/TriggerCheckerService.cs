using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Factories.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace EonWatchesAPI.Services.Services;

public class TriggerCheckerService : BackgroundService
{
    private readonly ILogger<TriggerCheckerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private DateTime _lastCheck = DateTime.UtcNow.Date.AddDays(-50);

    
    public TriggerCheckerService(ILogger<TriggerCheckerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🟢 NewAdWatcherService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;

                var triggers = await db.Triggers
                    .ToListAsync(stoppingToken);
                
                var newAds = await db.Ads
                    .Where(ad => ad.CreatedAt > _lastCheck)
                    .ToListAsync(stoppingToken);
             
                if (newAds.Any())
                {
                    _logger.LogInformation("🆕 Found {Count} new ad(s)", newAds.Count);

                    foreach (var ad in newAds)
                    {
                        // TODO: Add trigger matching + notification logic here
                        _logger.LogInformation("🔔 Ad #{AdId} for brand {Brand}", ad.Id, ad.Brand);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while checking for new ads");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("🔴 NewAdWatcherService stopped");
    }
}
