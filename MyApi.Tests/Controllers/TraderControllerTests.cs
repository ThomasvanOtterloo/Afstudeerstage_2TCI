using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using EonWatchesAPI.Controllers;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;

namespace MyApi.Tests.Controllers
{
    public class TraderControllerTests
    {
        private readonly Mock<ITraderService> _traderServiceMock = new();
        private readonly TraderController _controller;

        public TraderControllerTests()
        {
            _controller = new TraderController(_traderServiceMock.Object);
        }

        [Fact]
        public async Task GetTraders_ReturnsAllTraders()
        {
            // Arrange
            var traders = new List<Trader>
            {
                new Trader { Id = 1, Name = "Alice", Email = "a@example.com", PhoneNumber = "123" },
                new Trader { Id = 2, Name = "Bob",   Email = "b@example.com", PhoneNumber = "456" }
            };
            _traderServiceMock
                .Setup(s => s.GetTraders())
                .ReturnsAsync(traders);

            // Act
            IEnumerable<Trader> result = await _controller.GetTraders();

            // Assert
            result.Should().BeEquivalentTo(traders);
            _traderServiceMock.Verify(s => s.GetTraders(), Times.Once);
        }

        [Fact]
        public async Task GetTraderById_ReturnsTrader_WhenExists()
        {
            // Arrange
            var trader = new Trader { Id = 42, Name = "Carol", Email = "c@example.com", PhoneNumber = "789" };
            _traderServiceMock
                .Setup(s => s.GetTraderById(42))
                .ReturnsAsync(trader);

            // Act
            Trader result = await _controller.GetTraderById(42);

            // Assert
            result.Should().BeEquivalentTo(trader);
            _traderServiceMock.Verify(s => s.GetTraderById(42), Times.Once);
        }

        [Fact]
        public async Task CreateTrader_ReturnsCreatedTrader()
        {
            // Arrange
            var dto = new TraderDto
            {
                Name = "Dave",
                Email = "d@example.com",
                Password = "secret",
                PhoneNumber = "000"
            };
            var created = new Trader { Id = 99, Name = dto.Name, Email = dto.Email, PhoneNumber = dto.PhoneNumber };
            _traderServiceMock
                .Setup(s => s.CreateTrader(dto))
                .ReturnsAsync(created);

            // Act
            Trader result = await _controller.CreateTrader(dto);

            // Assert
            result.Should().BeEquivalentTo(created);
            _traderServiceMock.Verify(s => s.CreateTrader(dto), Times.Once);
        }
    }
}
