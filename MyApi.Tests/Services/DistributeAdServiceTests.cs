using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using EonWatchesAPI.Services.Services;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.Factories;

namespace MyApi.Tests.Services
{
    public class DistributeAdServiceTests
    {
        // Helper to create a mock IFormFile from raw bytes
        private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.Length).Returns(bytes.Length);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns((Stream target, System.Threading.CancellationToken _) =>
                {
                    stream.Position = 0;
                    return stream.CopyToAsync(target);
                });
            return mock.Object;
        }

        [Fact]
        public async Task SendMessageToGroup_CallsEachStrategy_WhenAllConnectionTypesSupported()
        {
            // Arrange
            var dto = new SendMessageDto
            {
                BearerToken = "tokenABC",
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                Text = "Hello everyone!",
                GroupIds = new List<string> { "G1", "G2" }
            };

            // Create mocks for each supported connection
            var waMock = new Mock<ISocialConnection>();
            var redditMock = new Mock<ISocialConnection>();

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object },
                { ConnectionType.Reddit, redditMock.Object }
            };

            var service = new DistributeAdService(strategies);

            // Act
            await service.SendMessageToGroup(dto);

            // Assert: each mock's SendTextToGroups called once with correct arguments
            waMock.Verify(s => s.SendTextToGroups(
                    "tokenABC",
                    "Hello everyone!",
                    It.Is<List<string>>(list => list.Count == 2 && list.Contains("G1") && list.Contains("G2"))
                ), Times.Once);

            redditMock.Verify(s => s.SendTextToGroups(
                    "tokenABC",
                    "Hello everyone!",
                    It.Is<List<string>>(list => list.Count == 2 && list.Contains("G1") && list.Contains("G2"))
                ), Times.Once);
        }

        [Fact]
        public async Task SendMessageToGroup_ThrowsNotSupportedException_WhenConnectionTypeMissing()
        {
            // Arrange: only WhatsApp in dictionary, but dto requests Signal
            var waMock = new Mock<ISocialConnection>();
            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object }
            };
            var service = new DistributeAdService(strategies);

            var dto = new SendMessageDto
            {
                BearerToken = "tokenXYZ",
                ConnectionType = new[] { ConnectionType.Signal }, // unsupported
                Text = "Test",
                GroupIds = new List<string> { "G1" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => service.SendMessageToGroup(dto));
            waMock.Verify(s => s.SendTextToGroups(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task SendImageToGroup_CallsEachStrategy_WithCorrectDataUri()
        {
            // Arrange
            var imageBytes = Encoding.UTF8.GetBytes("fake-image-bytes");
            var formFile = CreateMockFormFile("image.png", "image/png", imageBytes);

            var dto = new SendImageCaptionDto
            {
                BearerToken = "Bearer123",
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                Text = "Caption text",
                Image = formFile,
                GroupIds = new List<string> { "G1", "G2" }
            };

            var waMock = new Mock<ISocialConnection>();
            var redditMock = new Mock<ISocialConnection>();

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object },
                { ConnectionType.Reddit, redditMock.Object }
            };

            var service = new DistributeAdService(strategies);

            // Act
            await service.SendImageToGroup(dto);

            // Assert: construct expected data URI prefix
            var expectedPrefix = "data:image/png;base64,";
            waMock.Verify(s => s.SendImageToGroups(
                    "Bearer123",
                    "Caption text",
                    It.Is<string>(uri => uri.StartsWith(expectedPrefix)),
                    It.Is<List<string>>(list => list.Count == 2 && list.Contains("G1") && list.Contains("G2"))
                ), Times.Once);

            redditMock.Verify(s => s.SendImageToGroups(
                    "Bearer123",
                    "Caption text",
                    It.Is<string>(uri => uri.StartsWith(expectedPrefix)),
                    It.Is<List<string>>(list => list.Count == 2 && list.Contains("G1") && list.Contains("G2"))
                ), Times.Once);
        }

        [Fact]
        public async Task SendImageToGroup_ThrowsNotSupportedException_WhenConnectionTypeMissing()
        {
            // Arrange: strategies only has WhatsApp, but dto requests Signal
            var waMock = new Mock<ISocialConnection>();
            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object }
            };
            var service = new DistributeAdService(strategies);

            var imageBytes = Encoding.UTF8.GetBytes("fake");
            var formFile = CreateMockFormFile("img.jpg", "image/jpeg", imageBytes);

            var dto = new SendImageCaptionDto
            {
                BearerToken = "BearerX",
                ConnectionType = new[] { ConnectionType.Signal }, // unsupported
                Text = "Caption",
                Image = formFile,
                GroupIds = new List<string> { "G1" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => service.SendImageToGroup(dto));
            waMock.Verify(s => s.SendImageToGroups(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
        }
    }
}
