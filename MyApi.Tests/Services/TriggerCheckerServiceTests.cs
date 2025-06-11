using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.Factories.Notifications;
using EonWatchesAPI.Services.Services;

namespace MyApi.Tests.Services
{
    public class TriggerCheckerServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly AppDbContext _dbContext;
        private readonly Mock<INotification> _notificationMock;
        private readonly TestableTriggerCheckerService _service;
        private readonly string _inMemoryDbName = Guid.NewGuid().ToString();

        public TriggerCheckerServiceTests()
        {
            // 1) Build an in-memory service collection
            var services = new ServiceCollection();

            // 2) Register DbContext with unique in-memory database
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_inMemoryDbName));

            // 3) Create and register a Mock<INotification>
            _notificationMock = new Mock<INotification>();
            services.AddSingleton<INotification>(_notificationMock.Object);

            // 4) Build the provider
            _serviceProvider = services.BuildServiceProvider();

            // 5) Resolve the DbContext and seed data
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            SeedDatabase();

            // 6) Instantiate the testable service with a NullLogger
            var logger = new NullLogger<TriggerCheckerService>();
            _service = new TestableTriggerCheckerService(logger, _serviceProvider);
        }

        private void SeedDatabase()
        {
            // Create a Trader
            var trader = new Trader
            {
                Name = "TestTrader",
                Email = "test@trader.com",
                PhoneNumber = "555-1234"
            };
            _dbContext.Traders.Add(trader);
            _dbContext.SaveChanges();

            // Create a Trigger that matches brand "BrandX"
            var trigger = new Trigger
            {
                Brand = "BrandX",
                Model = null,
                ReferenceNumber = null,
                TraderId = trader.Id
            };
            _dbContext.Triggers.Add(trigger);

            // Create an Ad (CreatedAt = now, so > default lastCheck)
            var ad = new Ad
            {
                Id = 1,
                Brand = "BrandX ModelY",
                Model = "ModelY",
                ReferenceNumber = "REF123",
                CreatedAt = DateTime.UtcNow,
                TraderName = "SellerName",
                PhoneNumber = "555-0000"
            };
            _dbContext.Ads.Add(ad);

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task ExecuteOnceAsync_ShouldSendNotification_WhenAdMatchesTrigger()
        {
            // Arrange: we want to know the moment SendNotification is called.
            var tcs = new TaskCompletionSource<bool>();

            _notificationMock
                .Setup(n => n.SendNotification(It.IsAny<SendEmailRequest>()))
                .Callback(() => tcs.TrySetResult(true))
                .Returns(Task.CompletedTask);

            // Use a CancellationTokenSource that we will cancel manually
            var cts = new CancellationTokenSource();

            // Act: fire the protected ExecuteAsync (wrapped by ExecuteOnceAsync).
            var execTask = _service.ExecuteOnceAsync(cts.Token);

            // Wait until either (a) our TCS is signaled, or (b) a 5s timeout.
            var signaledTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (signaledTask == tcs.Task)
            {
                // As soon as we see SendNotification invoked, cancel the service.
                cts.Cancel();
            }
            else
            {
                // If 5 seconds pass without a notification, fail the test.
                cts.Cancel();
                throw new Exception("Timed out waiting for SendNotification to be called");
            }

            // Now await the service task to finish (will throw TaskCanceled, which we swallow)
            try
            {
                await execTask;
            }
            catch (TaskCanceledException)
            {
                // expected once we call cts.Cancel()
            }

            // Assert: the mock should have been called at least once
            _notificationMock.Verify(n =>
                n.SendNotification(It.IsAny<SendEmailRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteOnceAsync_ShouldNotSendNotification_WhenNoMatchingAd()
        {
            // Arrange: remove all ads and add a non-matching one
            _dbContext.Ads.RemoveRange(_dbContext.Ads);
            var nonMatchingAd = new Ad
            {
                Id = 2,
                Brand = "OtherBrand",
                Model = "ModelZ",
                ReferenceNumber = "REF999",
                CreatedAt = DateTime.UtcNow,
                TraderName = "SellerName",
                PhoneNumber = "555-0000"
            };
            _dbContext.Ads.Add(nonMatchingAd);
            _dbContext.SaveChanges();

            // This time we’ll wait a short period, then cancel if still not signaled.
            var tcs = new TaskCompletionSource<bool>();
            _notificationMock
                .Setup(n => n.SendNotification(It.IsAny<SendEmailRequest>()))
                .Callback(() => tcs.TrySetResult(true))
                .Returns(Task.CompletedTask);

            var cts = new CancellationTokenSource();

            // Act
            var execTask = _service.ExecuteOnceAsync(cts.Token);

            // If SendNotification is called within 5s, tcs.Task will complete. Otherwise timeout.
            var signaledTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (signaledTask == tcs.Task)
            {
                // Unexpected: if this fires, someone called SendNotification
                cts.Cancel();
                throw new Exception("SendNotification was called even though no ad matched");
            }
            else
            {
                // After 5s with no notification, cancel the service.
                cts.Cancel();
                try
                {
                    await execTask;
                }
                catch (TaskCanceledException)
                {
                    // expected
                }

                // Assert: the mock should never have been called
                _notificationMock.Verify(n =>
                    n.SendNotification(It.IsAny<SendEmailRequest>()),
                    Times.Never);
            }
        }

        public void Dispose()
        {
            // Clean up the in-memory database
            _dbContext.Database.EnsureDeleted();
            _serviceProvider.Dispose();
        }

        /// <summary>
        /// Exposes the protected ExecuteAsync as a public method for testing.
        /// </summary>
        private class TestableTriggerCheckerService : TriggerCheckerService
        {
            public TestableTriggerCheckerService(
                Microsoft.Extensions.Logging.ILogger<TriggerCheckerService> logger,
                IServiceProvider serviceProvider)
                : base(logger, serviceProvider)
            {
            }

            public Task ExecuteOnceAsync(CancellationToken stoppingToken)
            {
                return base.ExecuteAsync(stoppingToken);
            }
        }
    }
}
