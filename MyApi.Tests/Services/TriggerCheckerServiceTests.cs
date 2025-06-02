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
        private readonly TriggerCheckerService _service;
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

            // 4) Build the provider
            _serviceProvider = services.BuildServiceProvider();

            // 5) Resolve the DbContext and seed data
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            SeedDatabase();

            // 6) Instantiate the service with a NullLogger and the ServiceProvider
            var logger = new NullLogger<TriggerCheckerService>();
            _service = new TriggerCheckerService(logger, _serviceProvider);
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

            // Create an Ad with CreatedAt > _lastCheck (default is 50 days ago)
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
        public async Task StartAsync_ShouldSendNotification_WhenAdMatchesTrigger()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            // Cancel after 200ms so StartAsync only runs one iteration
            cts.CancelAfter(TimeSpan.FromMilliseconds(200));

            // Act
            await _service.StartAsync(cts.Token);

            // Assert: Notification.SendNotification should be called at least once
            _notificationMock.Verify(n =>
                n.SendNotification(It.IsAny<SendEmailRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task StartAsync_ShouldNotSendNotification_WhenNoMatchingAd()
        {
            // Arrange: remove the existing Ad and add a non-matching one
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
            await _service.StartAsync(cts.Token);

            // Assert: No notifications should have been sent
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
    }
}
