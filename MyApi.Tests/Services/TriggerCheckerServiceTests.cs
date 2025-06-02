using System;
using System.Linq;
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
            services.AddSingleton(_notificationMock.Object);

            // 4) Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // 5) Resolve the DbContext and seed data
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            SeedDatabase();

            // 6) Instantiate the *testable* service with a NullLogger
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

            // Create a Trigger matching brand "BrandX"
            var trigger = new Trigger
            {
                Brand = "BrandX",
                Model = null,
                ReferenceNumber = null,
                TraderId = trader.Id
            };
            _dbContext.Triggers.Add(trigger);

            // Create an Ad (CreatedAt = now, so it is > default _lastCheck)
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
            // Arrange: cancel after a short delay so that the loop runs exactly once
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            // Act: call the protected ExecuteAsync via our test subclass
            try
            {
                await _service.ExecuteOnceAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // We expect cancellation; swallow it
            }

            // Assert: Notification.SendNotification must have been called at least once
            _notificationMock.Verify(n =>
                n.SendNotification(It.IsAny<SendEmailRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteOnceAsync_ShouldNotSendNotification_WhenNoMatchingAd()
        {
            // Arrange: remove all Ads and add a non-matching one
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

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            // Act
            try
            {
                await _service.ExecuteOnceAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // swallow
            }

            // Assert: no notifications sent
            _notificationMock.Verify(n =>
                n.SendNotification(It.IsAny<SendEmailRequest>()),
                Times.Never);
        }

        public void Dispose()
        {
            // Clean up the in-memory database
            _dbContext.Database.EnsureDeleted();
            _serviceProvider.Dispose();
        }

        /// <summary>
        /// A small test‐only subclass that exposes the protected ExecuteAsync as a public method.
        /// </summary>
        private class TestableTriggerCheckerService : TriggerCheckerService
        {
            public TestableTriggerCheckerService(
                Microsoft.Extensions.Logging.ILogger<TriggerCheckerService> logger,
                IServiceProvider serviceProvider)
                : base(logger, serviceProvider)
            {
            }

            // Expose the protected ExecuteAsync so our tests can call it directly:
            public Task ExecuteOnceAsync(CancellationToken stoppingToken)
            {
                return base.ExecuteAsync(stoppingToken);
            }
        }
    }
}
