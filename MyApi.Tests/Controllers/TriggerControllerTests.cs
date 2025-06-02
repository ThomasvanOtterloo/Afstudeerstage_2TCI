using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using EonWatchesAPI.Controllers;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.Notifications;
using EonWatchesAPI.Services.I_Services;

namespace MyApi.Tests.Controllers
{
    public class TriggerControllerTests
    {
        private readonly Mock<ITriggerService> _triggerServiceMock = new();
        private readonly Mock<INotification> _notificationMock = new();
        private readonly TriggerController _controller;

        public TriggerControllerTests()
        {
            _controller = new TriggerController(_triggerServiceMock.Object, _notificationMock.Object);
        }

        [Fact]
        public async Task GetTriggers_ReturnsAllTriggers()
        {
            // Arrange
            var triggers = new List<Trigger>
            {
                new Trigger { Id = 1, ReferenceNumber = "T1" },
                new Trigger { Id = 2, ReferenceNumber = "T2" }
            };
            _triggerServiceMock
                .Setup(s => s.GetTriggers())
                .ReturnsAsync(triggers);

            // Act
            IEnumerable<Trigger> result = await _controller.GetTriggers();

            // Assert
            result.Should().BeEquivalentTo(triggers);
            _triggerServiceMock.Verify(s => s.GetTriggers(), Times.Once);
        }

        [Fact]
        public async Task GetTriggerById_ReturnsTrigger_WhenExists()
        {
            // Arrange
            var trigger = new Trigger { Id = 42, ReferenceNumber = "Sample" };
            _triggerServiceMock
                .Setup(s => s.GetTriggerById(42))
                .ReturnsAsync(trigger);

            // Act
            Trigger result = await _controller.GetTriggerById(42);

            // Assert
            result.Should().BeEquivalentTo(trigger);
            _triggerServiceMock.Verify(s => s.GetTriggerById(42), Times.Once);
        }

        [Fact]
        public async Task GetTriggerById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _triggerServiceMock
                .Setup(s => s.GetTriggerById(100))
                .ReturnsAsync((Trigger)null!);

            // Act
            Trigger result = await _controller.GetTriggerById(100);

            // Assert
            result.Should().BeNull();
            _triggerServiceMock.Verify(s => s.GetTriggerById(100), Times.Once);
        }

        [Fact]
        public async Task GetTriggerListByUserId_ReturnsUserTriggers()
        {
            // Arrange
            var userTriggers = new List<Trigger>
            {
                new Trigger { Id = 1, ReferenceNumber = "A", TraderId = 5 },
                new Trigger { Id = 2, ReferenceNumber = "B", TraderId = 5 }
            };
            _triggerServiceMock
                .Setup(s => s.GetTriggerListByUserId(5))
                .ReturnsAsync(userTriggers);

            // Act
            List<Trigger> result = await _controller.GetTriggerListByUserId(5);

            // Assert
            result.Should().BeEquivalentTo(userTriggers);
            _triggerServiceMock.Verify(s => s.GetTriggerListByUserId(5), Times.Once);
        }

        [Fact]
        public async Task CreateTrigger_ReturnsCreatedTrigger()
        {
            // Arrange
            var dto = new TriggerCreateDto { ReferenceNumber = "NewTrigger", TraderId = 10 };
            var created = new Trigger { Id = 99, ReferenceNumber = dto.ReferenceNumber, TraderId = dto.TraderId };
            _triggerServiceMock
                .Setup(s => s.CreateTrigger(dto))
                .ReturnsAsync(created);

            // Act
            Trigger result = await _controller.CreateTrigger(dto);

            // Assert
            result.Should().BeEquivalentTo(created);
            _triggerServiceMock.Verify(s => s.CreateTrigger(dto), Times.Once);
        }

        [Fact]
        public async Task CreateTriggerMail_ReturnsSuccessMessage_WhenNotificationSucceeds()
        {
            // Arrange
            var mailRequest = new SendEmailRequest("Hi", "Hello", "a@b.com");
            _notificationMock
                .Setup(n => n.SendNotification(mailRequest))
                .Returns(Task.CompletedTask);

            // Act
            string result = await _controller.CreateTrigger(mailRequest);

            // Assert
            result.Should().Be("Email sent successfully");
            _notificationMock.Verify(n => n.SendNotification(mailRequest), Times.Once);
        }


        [Fact]
        public async Task DeleteTrigger_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            _triggerServiceMock
                .Setup(s => s.DeleteTrigger(5))
                .ReturnsAsync(true);

            // Act
            IActionResult actionResult = await _controller.DeleteTrigger(5);

            // Assert
            actionResult.Should().BeOfType<NoContentResult>();
            _triggerServiceMock.Verify(s => s.DeleteTrigger(5), Times.Once);
        }

        [Fact]
        public async Task DeleteTrigger_ReturnsNotFound_WhenNotDeleted()
        {
            // Arrange
            _triggerServiceMock
                .Setup(s => s.DeleteTrigger(10))
                .ReturnsAsync(false);

            // Act
            IActionResult actionResult = await _controller.DeleteTrigger(10);

            // Assert
            actionResult
                .Should()
                .BeOfType<NotFoundObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(new { message = $"No trigger found with ID 10." });
            _triggerServiceMock.Verify(s => s.DeleteTrigger(10), Times.Once);
        }

        [Fact]
        public async Task DeleteTrigger_ReturnsServerError_WhenServiceThrows()
        {
            // Arrange
            _triggerServiceMock
                .Setup(s => s.DeleteTrigger(7))
                .ThrowsAsync(new Exception("Failure"));

            // Act
            IActionResult actionResult = await _controller.DeleteTrigger(7);

            // Assert
            actionResult
                .Should()
                .BeOfType<ObjectResult>()
                .Which.StatusCode
                .Should()
                .Be(500);
            _triggerServiceMock.Verify(s => s.DeleteTrigger(7), Times.Once);
        }
    }
}
