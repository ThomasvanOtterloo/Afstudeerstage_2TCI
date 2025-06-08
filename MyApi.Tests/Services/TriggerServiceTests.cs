using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.Services;

namespace MyApi.Tests.Services
{
    public class TriggerServiceTests
    {
        private readonly Mock<ITriggerRepository> _triggerRepoMock = new();
        private readonly TriggerService _service;

        public TriggerServiceTests()
        {
            _service = new TriggerService(_triggerRepoMock.Object);
        }

        [Fact]
        public async Task GetTriggers_ReturnsAllTriggers()
        {
            // Arrange
            var triggers = new List<Trigger>
            {
                new Trigger { Id = 1, Brand = "BrandA", Model = "ModelA", ReferenceNumber = "REF1", TraderId = 10 },
                new Trigger { Id = 2, Brand = "BrandB", Model = "ModelB", ReferenceNumber = "REF2", TraderId = 20 }
            };
            _triggerRepoMock
                .Setup(r => r.GetTriggers())
                .ReturnsAsync(triggers);

            // Act
            var result = await _service.GetTriggers();

            // Assert
            result.Should().BeEquivalentTo(triggers);
            _triggerRepoMock.Verify(r => r.GetTriggers(), Times.Once);
        }

        [Fact]
        public async Task GetTriggerById_ReturnsSingleTrigger()
        {
            // Arrange
            var trigger = new Trigger { Id = 42, Brand = "BrandX", Model = "ModelX", ReferenceNumber = "REFX", TraderId = 99 };
            _triggerRepoMock
                .Setup(r => r.GetTriggerById(42))
                .ReturnsAsync(trigger);

            // Act
            var result = await _service.GetTriggerById(42);

            // Assert
            result.Should().BeEquivalentTo(trigger);
            _triggerRepoMock.Verify(r => r.GetTriggerById(42), Times.Once);
        }

        [Fact]
        public async Task GetTriggerListByUserId_ReturnsUserTriggerList()
        {
            // Arrange
            int userId = 5;
            var userTriggers = new List<Trigger>
            {
                new Trigger { Id = 11, Brand = "Brand1", Model = "Model1", ReferenceNumber = "REF1", TraderId = userId },
                new Trigger { Id = 12, Brand = "Brand2", Model = "Model2", ReferenceNumber = "REF2", TraderId = userId }
            };
            _triggerRepoMock
                .Setup(r => r.GetTriggerListByUserId(userId))
                .ReturnsAsync(userTriggers);

            // Act
            var result = await _service.GetTriggerListByUserId(userId);

            // Assert
            result.Should().BeEquivalentTo(userTriggers);
            _triggerRepoMock.Verify(r => r.GetTriggerListByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task CreateTrigger_CreatesAndReturnsNewTrigger()
        {
            // Arrange
            var dto = new TriggerCreateDto
            {
                TraderId = 7,
                Brand = "NewBrand",
                Model = "NewModel",
                ReferenceNumber = "NEWREF"
            };
            // We expect the service to construct a Trigger entity with these values:
            var createdTrigger = new Trigger
            {
                Id = 100,
                TraderId = dto.TraderId,
                Brand = dto.Brand,
                Model = dto.Model,
                ReferenceNumber = dto.ReferenceNumber
            };
            _triggerRepoMock
                .Setup(r => r.CreateTrigger(It.Is<Trigger>(t =>
                    t.TraderId == dto.TraderId &&
                    t.Brand == dto.Brand &&
                    t.Model == dto.Model &&
                    t.ReferenceNumber == dto.ReferenceNumber
                )))
                .ReturnsAsync(createdTrigger);

            // Act
            var result = await _service.CreateTrigger(dto);

            // Assert
            result.Should().BeEquivalentTo(createdTrigger);
            _triggerRepoMock.Verify(r => r.CreateTrigger(It.IsAny<Trigger>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTrigger_ReturnsTrue_WhenRepositoryDeletes()
        {
            // Arrange
            int idToDelete = 50;
            _triggerRepoMock
                .Setup(r => r.DeleteTrigger(idToDelete))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteTrigger(idToDelete);

            // Assert
            result.Should().BeTrue();
            _triggerRepoMock.Verify(r => r.DeleteTrigger(idToDelete), Times.Once);
        }

        [Fact]
        public async Task DeleteTrigger_ReturnsFalse_WhenRepositoryFails()
        {
            // Arrange
            int idToDelete = 51;
            _triggerRepoMock
                .Setup(r => r.DeleteTrigger(idToDelete))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteTrigger(idToDelete);

            // Assert
            result.Should().BeFalse();
            _triggerRepoMock.Verify(r => r.DeleteTrigger(idToDelete), Times.Once);
        }
    }
}
