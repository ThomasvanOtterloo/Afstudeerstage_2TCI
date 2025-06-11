using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
        public async Task Distribute_ReturnsOk_WhenServiceSendsMessage()
        {
            // Arrange
            var dto = new DistributeAdDto
            {
                Token = 123,
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto() // no Image -> message branch
            };

            _distributeAdServiceMock
                .Setup(s => s.SendMessageToGroup(
                    It.Is<DistributeAdDto>(d =>
                        d.Token == dto.Token &&
                        d.GroupIds.Count == dto.GroupIds.Count &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.WhatsApp &&
                        d.ConnectionType[1] == ConnectionType.Reddit &&
                        d.AdEntities != null &&
                        d.AdEntities.Image == null
                    )
                ))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.Distribute(dto);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _distributeAdServiceMock.Verify(s =>
                s.SendMessageToGroup(It.Is<DistributeAdDto>(d =>
                    d.Token == dto.Token &&
                    d.GroupIds.Count == dto.GroupIds.Count &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.WhatsApp &&
                    d.ConnectionType[1] == ConnectionType.Reddit &&
                    d.AdEntities != null &&
                    d.AdEntities.Image == null
                )), Times.Once);
        }

        [Fact]
        public async Task Distribute_ReturnsBadRequest_WhenServiceThrowsOnMessage()
        {
            // Arrange
            var dto = new DistributeAdDto
            {
                Token = 456,
                ConnectionType = new[] { ConnectionType.Signal },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto()
            };
            var exceptionMessage = "Service failure";

            _distributeAdServiceMock
                .Setup(s => s.SendMessageToGroup(
                    It.Is<DistributeAdDto>(d =>
                        d.Token == dto.Token &&
                        d.ConnectionType.Length == 1 &&
                        d.ConnectionType[0] == ConnectionType.Signal
                    )
                ))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.Distribute(dto);

            // Assert
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _distributeAdServiceMock.Verify(s =>
                s.SendMessageToGroup(It.Is<DistributeAdDto>(d =>
                    d.Token == dto.Token &&
                    d.ConnectionType.Length == 1 &&
                    d.ConnectionType[0] == ConnectionType.Signal
                )), Times.Once);
        }

        [Fact]
        public async Task Distribute_ReturnsOk_WhenServiceSendsImage()
        {
            // Arrange
            var formFile = CreateMockFormFile(
                fileName: "test.png",
                contentType: "image/png",
                bytes: Encoding.UTF8.GetBytes("dummy")
            );

            var dto = new DistributeAdDto
            {
                Token = 789,
                ConnectionType = new[] { ConnectionType.Reddit },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto { Image = formFile }
            };

            _distributeAdServiceMock
                .Setup(s => s.SendImageToGroup(
                    It.Is<DistributeAdDto>(d =>
                        d.Token == dto.Token &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.Reddit &&
                        d.AdEntities != null &&
                        d.AdEntities.Image == formFile
                    )
                ))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.Distribute(dto);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _distributeAdServiceMock.Verify(s =>
                s.SendImageToGroup(It.Is<DistributeAdDto>(d =>
                    d.Token == dto.Token &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.Reddit &&
                    d.AdEntities != null &&
                    d.AdEntities.Image == formFile
                )), Times.Once);
        }

        [Fact]
        public async Task Distribute_ReturnsBadRequest_WhenServiceThrowsOnImage()
        {
            // Arrange
            var formFile = CreateMockFormFile(
                fileName: "test.png",
                contentType: "image/png",
                bytes: Encoding.UTF8.GetBytes("dummy")
            );

            var dto = new DistributeAdDto
            {
                Token = 999,
                ConnectionType = new[] { ConnectionType.Signal, ConnectionType.WhatsApp },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto { Image = formFile }
            };

            var exceptionMessage = "Image service failed";
            _distributeAdServiceMock
                .Setup(s => s.SendImageToGroup(
                    It.Is<DistributeAdDto>(d =>
                        d.Token == dto.Token &&
                        d.ConnectionType.Length == dto.ConnectionType.Length &&
                        d.ConnectionType[0] == ConnectionType.Signal &&
                        d.ConnectionType[1] == ConnectionType.WhatsApp &&
                        d.AdEntities != null &&
                        d.AdEntities.Image == formFile
                    )
                ))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.Distribute(dto);

            // Assert
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _distributeAdServiceMock.Verify(s =>
                s.SendImageToGroup(It.Is<DistributeAdDto>(d =>
                    d.Token == dto.Token &&
                    d.ConnectionType.Length == dto.ConnectionType.Length &&
                    d.ConnectionType[0] == ConnectionType.Signal &&
                    d.ConnectionType[1] == ConnectionType.WhatsApp &&
                    d.AdEntities != null &&
                    d.AdEntities.Image == formFile
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

        //[Fact]
        //public void DistributeAdDto_MissingToken_ShouldProduceValidationError()
        //{
        //    // Arrange
        //    var dto = new DistributeAdDto
        //    {
        //        // Token not set
        //        ConnectionType = new[] { ConnectionType.WhatsApp },
        //        GroupIds = new List<string> { "G1" },
        //        AdEntities = new CreateAdDto()
        //    };

        //    // Act
        //    var results = ValidateModel(dto);

        //    // Assert
        //    Assert.Contains(results, r =>
        //        r.MemberNames.Contains(nameof(DistributeAdDto.Token)) &&
        //        r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
        //    );
        //}

        [Fact]
        public void DistributeAdDto_MissingConnectionType_ShouldProduceValidationError()
        {
            // Arrange
            var dto = new DistributeAdDto
            {
                Token = 123,
                ConnectionType = null!,
                GroupIds = new List<string> { "G1" },
                AdEntities = new CreateAdDto()
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(DistributeAdDto.ConnectionType)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void DistributeAdDto_EmptyGroupIds_ShouldProduceMinLengthValidationError()
        {
            // Arrange
            var dto = new DistributeAdDto
            {
                Token = 123,
                ConnectionType = new[] { ConnectionType.Reddit },
                GroupIds = new List<string>(), // empty
                AdEntities = new CreateAdDto()
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(DistributeAdDto.GroupIds)) &&
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
