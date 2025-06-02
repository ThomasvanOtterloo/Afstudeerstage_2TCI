using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using EonWatchesAPI.Controllers;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.Services.I_Services;

namespace MyApi.Tests.Controllers
{
    public class DistributeAdControllerTests
    {
        private readonly Mock<IDistributeAdService> _distributeAdServiceMock = new();
        private readonly DistributeAdController _controller;

        public DistributeAdControllerTests()
        {
            _controller = new DistributeAdController(_distributeAdServiceMock.Object);
        }

        // ------------- Controller Tests -------------

        [Fact]
        public async Task SendMessage_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = "token123",
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                Text = "Hello Group",
                GroupIds = new List<string> { "G1", "G2" }
            };

            _distributeAdServiceMock
                .Setup(s => s.SendMessageToGroup(
                    It.Is<SendMessageDto>(d =>
                        d.BearerToken == dto.BearerToken &&
                        d.Text == dto.Text &&
                        d.GroupIds.Count == dto.GroupIds.Count &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.WhatsApp &&
                        d.ConnectionType[1] == ConnectionType.Reddit
                    )
                ))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.SendMessage(dto);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _distributeAdServiceMock.Verify(s =>
                s.SendMessageToGroup(It.Is<SendMessageDto>(d =>
                    d.BearerToken == dto.BearerToken &&
                    d.Text == dto.Text &&
                    d.GroupIds.Count == dto.GroupIds.Count &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.WhatsApp &&
                    d.ConnectionType[1] == ConnectionType.Reddit
                )), Times.Once);
        }

        [Fact]
        public async Task SendMessage_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = "token123",
                ConnectionType = new[] { ConnectionType.Signal },
                Text = "Hello Group",
                GroupIds = new List<string> { "G1", "G2" }
            };
            var exceptionMessage = "Service failure";

            _distributeAdServiceMock
                .Setup(s => s.SendMessageToGroup(
                    It.Is<SendMessageDto>(d =>
                        d.BearerToken == dto.BearerToken &&
                        d.ConnectionType.Length == 1 &&
                        d.ConnectionType[0] == ConnectionType.Signal
                    )
                ))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.SendMessage(dto);

            // Assert
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _distributeAdServiceMock.Verify(s =>
                s.SendMessageToGroup(It.Is<SendMessageDto>(d =>
                    d.BearerToken == dto.BearerToken &&
                    d.ConnectionType.Length == 1 &&
                    d.ConnectionType[0] == ConnectionType.Signal
                )), Times.Once);
        }

        [Fact]
        public async Task SendMessageWithImage_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var formFile = CreateMockFormFile(
                fileName: "test.png",
                contentType: "image/png",
                bytes: Encoding.UTF8.GetBytes("dummy")
            );

            var dto = new SendImageCaptionDto
            {
                BearerToken = "jwt-token",
                ConnectionType = new[] { ConnectionType.Reddit },
                Text = "Picture Text",
                Image = formFile,
                GroupIds = new List<string> { "G1", "G2" }
            };

            _distributeAdServiceMock
                .Setup(s => s.SendImageToGroup(
                    It.Is<SendImageCaptionDto>(d =>
                        d.BearerToken == dto.BearerToken &&
                        d.Text == dto.Text &&
                        d.GroupIds.Count == dto.GroupIds.Count &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.Reddit &&
                        d.Image == formFile
                    )
                ))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.SendMessageWithImage(dto);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _distributeAdServiceMock.Verify(s =>
                s.SendImageToGroup(It.Is<SendImageCaptionDto>(d =>
                    d.BearerToken == dto.BearerToken &&
                    d.Text == dto.Text &&
                    d.GroupIds.Count == dto.GroupIds.Count &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.Reddit &&
                    d.Image == formFile
                )), Times.Once);
        }

        [Fact]
        public async Task SendMessageWithImage_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var formFile = CreateMockFormFile(
                fileName: "test.png",
                contentType: "image/png",
                bytes: Encoding.UTF8.GetBytes("dummy")
            );

            var dto = new SendImageCaptionDto
            {
                BearerToken = "jwt-token",
                ConnectionType = new[] { ConnectionType.Signal, ConnectionType.WhatsApp },
                Text = "Picture Text",
                Image = formFile,
                GroupIds = new List<string> { "G1", "G2" }
            };

            var exceptionMessage = "Image service failed";
            _distributeAdServiceMock
                .Setup(s => s.SendImageToGroup(
                    It.Is<SendImageCaptionDto>(d =>
                        d.BearerToken == dto.BearerToken &&
                        d.GroupIds.Count == dto.GroupIds.Count &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.Signal &&
                        d.ConnectionType[1] == ConnectionType.WhatsApp &&
                        d.Image == formFile
                    )
                ))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.SendMessageWithImage(dto);

            // Assert
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _distributeAdServiceMock.Verify(s =>
                s.SendImageToGroup(It.Is<SendImageCaptionDto>(d =>
                    d.BearerToken == dto.BearerToken &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.Signal &&
                    d.ConnectionType[1] == ConnectionType.WhatsApp &&
                    d.Image == formFile
                )), Times.Once);
        }

        // ------------- DTO Validation Tests -------------

        private IList<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void SendMessageDto_MissingBearerToken_ShouldProduceValidationError()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = null!,
                ConnectionType = new[] { ConnectionType.WhatsApp },
                Text = "Hello",
                GroupIds = new List<string> { "G1" }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendMessageDto.BearerToken)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void SendMessageDto_MissingConnectionType_ShouldProduceValidationError()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = "token123",
                ConnectionType = null!,
                Text = "Hello",
                GroupIds = new List<string> { "G1" }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendMessageDto.ConnectionType)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void SendMessageDto_EmptyGroupIds_ShouldProduceMinLengthValidationError()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = "token123",
                ConnectionType = new[] { ConnectionType.Reddit },
                Text = "Hello",
                GroupIds = new List<string>() // empty
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendMessageDto.GroupIds)) &&
                r.ErrorMessage!.Contains("must contain at least one group", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void SendImageCaptionDto_MissingBearerToken_ShouldProduceValidationError()
        {
            // Arrange
            var dto = new SendImageCaptionDto
            {
                BearerToken = null!,
                ConnectionType = new[] { ConnectionType.Signal },
                Text = "Caption",
                GroupIds = new List<string> { "G1" },
                Image = CreateMockFormFile("photo.png", "image/png", Encoding.UTF8.GetBytes("dummy"))
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendImageCaptionDto.BearerToken)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void SendImageCaptionDto_MissingConnectionType_ShouldProduceValidationError()
        {
            // Arrange
            var dto = new SendImageCaptionDto
            {
                BearerToken = "jwt-token",
                ConnectionType = null!,
                Text = "Caption",
                GroupIds = new List<string> { "G1" },
                Image = CreateMockFormFile("photo.png", "image/png", Encoding.UTF8.GetBytes("dummy"))
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendImageCaptionDto.ConnectionType)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void SendImageCaptionDto_EmptyGroupIds_ShouldProduceMinLengthValidationError()
        {
            // Arrange
            var dto = new SendImageCaptionDto
            {
                BearerToken = "jwt-token",
                ConnectionType = new[] { ConnectionType.Reddit },
                Text = "Caption",
                GroupIds = new List<string>(), // empty
                Image = CreateMockFormFile("photo.png", "image/png", Encoding.UTF8.GetBytes("dummy"))
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(SendImageCaptionDto.GroupIds)) &&
                r.ErrorMessage!.Contains("must contain at least one group", StringComparison.OrdinalIgnoreCase)
            );
        }

        private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var mock = new Mock<IFormFile>();

            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.Length).Returns(bytes.Length);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns((Stream target, System.Threading.CancellationToken _) =>
                {
                    stream.Position = 0;
                    return stream.CopyToAsync(target);
                });

            return mock.Object;
        }
    }
}
