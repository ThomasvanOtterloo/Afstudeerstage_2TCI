using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Factories.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
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
                var notifier = scope.ServiceProvider.GetRequiredService<INotification>();

                var now = DateTime.UtcNow;

                // load all your triggers once per loop
                var triggers = await db.Triggers
                    .Include(t => t.Trader)
                    .ToListAsync(stoppingToken);

                // find ads created since last check
                var newAds = await db.Ads
                    .Where(ad => ad.CreatedAt > _lastCheck)
                    .ToListAsync(stoppingToken);

                if (newAds.Any())
                {
                    //_logger.LogInformation("🆕 Found {Count} new ad(s)", newAds.Count);

                    foreach (var ad in newAds)
                    {
                        // find any triggers whose Brand, Model or ReferenceNumber
                        // appears in the new ad
                        var matches = triggers
                            .Where(t =>
                                (!string.IsNullOrWhiteSpace(t.Brand)
                                   && ad.Brand?.Contains(t.Brand, StringComparison.OrdinalIgnoreCase) == true)
                             || (!string.IsNullOrWhiteSpace(t.Model)
                                   && ad.Model?.Contains(t.Model, StringComparison.OrdinalIgnoreCase) == true)
                             || (!string.IsNullOrWhiteSpace(t.ReferenceNumber)
                                   && ad.ReferenceNumber?.Contains(t.ReferenceNumber, StringComparison.OrdinalIgnoreCase) == true)
                            )
                            .ToList();

                        if (matches.Any())
                        {
                            foreach (var trig in matches)
                            {
                                //_logger.LogInformation(
                                //  "🔔 Ad #{AdId} matches Trigger #{TriggerId} ({Brand}/{Model}/{Ref})",
                                //  ad.Id, trig.Id, trig.Brand, trig.Model, trig.ReferenceNumber);

                                var emailInfo = new SendEmailRequest(
                                    Subject: "Your Watch Trigger Fired!",
                                    Body: $"Hello {trig.Trader.Name},\n\n" +
                                                    $"An ad (# {ad.Id}) for “{ad.Brand} {ad.Model}” " +
                                                    $"with ref \"{ad.ReferenceNumber}\" just appeared " +
                                                    $"that matches your trigger settings.\n\n" +
                                                    $"Message {ad.TraderName} now at {ad.PhoneNumber} to see whats up! \n\n" +
                                                    "– EonWatches Bot",
                                    RecipientEmail: trig.Trader.Email
                                );
                                // fire your notification
                                notifier.SendNotification(emailInfo);
                            }
                        }
                    }

                    // advance the cutoff so you only process each ad once
                    _lastCheck = now;
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
