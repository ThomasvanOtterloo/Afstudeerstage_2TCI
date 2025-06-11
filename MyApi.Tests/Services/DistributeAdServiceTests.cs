using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using EonWatchesAPI.Services.Services;
using EonWatchesAPI.Services.I_Services;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Factories;

namespace MyApi.Tests.Services
{
    public class DistributeAdServiceTests
    {
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
            var traderId = 42;
            var bearerToken = "whapi-token";
            var dto = new DistributeAdDto
            {
                Token = traderId,
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto
                {
                    Other = "foo"  // ensures BuildTextPayload returns something non‐empty
                }
            };

            // strategy mocks
            var waMock = new Mock<ISocialConnection>();
            var redditMock = new Mock<ISocialConnection>();
            waMock
                .Setup(s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("msg1");
            redditMock
                .Setup(s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("msg2");

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object },
                { ConnectionType.Reddit, redditMock.Object }
            };

            // repo mocks
            var traderRepo = new Mock<ITraderRepository>();
            traderRepo
                .Setup(r => r.GetTraderById(traderId))
                .ReturnsAsync(new Trader { Id = traderId, WhapiBearerToken = bearerToken });

            var groupRepo = new Mock<IGroupRepository>();
            groupRepo
                .Setup(r => r.GetWhitelistedGroups(traderId))
                .ReturnsAsync(new[]
                {
                    new WhitelistedGroup { Id = "G1" },
                    new WhitelistedGroup { Id = "G2" }
                });

            var adRepo = new Mock<IAdRepository>();
            adRepo
                .Setup(r => r.CreateAd(It.IsAny<Ad>()))
                .Returns(Task.CompletedTask);

            var service = new DistributeAdService(
                strategies,
                traderRepo.Object,
                groupRepo.Object,
                adRepo.Object
            );

            // Act
            await service.SendMessageToGroup(dto);

            // Assert: each connection invoked once per group
            waMock.Verify(
                s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), "G1"),
                Times.Once);
            waMock.Verify(
                s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), "G2"),
                Times.Once);

            redditMock.Verify(
                s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), "G1"),
                Times.Once);
            redditMock.Verify(
                s => s.SendTextToGroup(bearerToken, It.IsAny<string>(), "G2"),
                Times.Once);

            // And we saved one Ad per message sent (2 connections × 2 groups)
            adRepo.Verify(r => r.CreateAd(It.IsAny<Ad>()), Times.Exactly(4));
        }

        [Fact]
        public async Task SendMessageToGroup_ThrowsNotSupportedException_WhenConnectionTypeMissing()
        {
            // Arrange
            var traderId = 99;
            var bearerToken = "bearer";
            var dto = new DistributeAdDto
            {
                Token = traderId,
                ConnectionType = new[] { ConnectionType.Signal },  // not in strategies
                GroupIds = new List<string> { "G1" },
                AdEntities = new CreateAdDto { Other = "x" }
            };

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, new Mock<ISocialConnection>().Object }
            };

            var traderRepo = new Mock<ITraderRepository>();
            traderRepo
                .Setup(r => r.GetTraderById(traderId))
                .ReturnsAsync(new Trader { Id = traderId, WhapiBearerToken = bearerToken });

            var groupRepo = new Mock<IGroupRepository>();
            groupRepo
                .Setup(r => r.GetWhitelistedGroups(traderId))
                .ReturnsAsync(new[] { new WhitelistedGroup { Id = "G1" } });

            var adRepo = new Mock<IAdRepository>();
            adRepo.Setup(r => r.CreateAd(It.IsAny<Ad>())).Returns(Task.CompletedTask);

            var service = new DistributeAdService(
                strategies,
                traderRepo.Object,
                groupRepo.Object,
                adRepo.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                service.SendMessageToGroup(dto));
        }

        [Fact]
        public async Task SendImageToGroup_CallsEachStrategy_WithCorrectDataUri()
        {
            // Arrange
            var traderId = 7;
            var bearerToken = "tok";
            var imageBytes = Encoding.UTF8.GetBytes("fake-image-bytes");
            var formFile = CreateMockFormFile("pic.png", "image/png", imageBytes);

            var dto = new DistributeAdDto
            {
                Token = traderId,
                ConnectionType = new[] { ConnectionType.WhatsApp, ConnectionType.Reddit },
                GroupIds = new List<string> { "G1", "G2" },
                AdEntities = new CreateAdDto { Image = formFile }
            };

            var waMock = new Mock<ISocialConnection>();
            var redditMock = new Mock<ISocialConnection>();
            waMock
                .Setup(s => s.SendImageToGroup(
                    bearerToken,
                    It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")),
                    It.IsAny<string>()
                ))
                .ReturnsAsync("m1");
            redditMock
                .Setup(s => s.SendImageToGroup(
                    bearerToken,
                    It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")),
                    It.IsAny<string>()
                ))
                .ReturnsAsync("m2");

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, waMock.Object },
                { ConnectionType.Reddit, redditMock.Object }
            };

            var traderRepo = new Mock<ITraderRepository>();
            traderRepo
                .Setup(r => r.GetTraderById(traderId))
                .ReturnsAsync(new Trader { Id = traderId, WhapiBearerToken = bearerToken });

            var groupRepo = new Mock<IGroupRepository>();
            groupRepo
                .Setup(r => r.GetWhitelistedGroups(traderId))
                .ReturnsAsync(new[]
                {
                    new WhitelistedGroup { Id = "G1" },
                    new WhitelistedGroup { Id = "G2" }
                });

            var adRepo = new Mock<IAdRepository>();
            adRepo.Setup(r => r.CreateAd(It.IsAny<Ad>())).Returns(Task.CompletedTask);

            var service = new DistributeAdService(
                strategies,
                traderRepo.Object,
                groupRepo.Object,
                adRepo.Object
            );

            // Act
            await service.SendImageToGroup(dto);

            // Assert: each connection × each group
            waMock.Verify(s =>
                s.SendImageToGroup(bearerToken, It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")), "G1"),
                Times.Once);

            waMock.Verify(s =>
                s.SendImageToGroup(bearerToken, It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")), "G2"),
                Times.Once);

            redditMock.Verify(s =>
                s.SendImageToGroup(bearerToken, It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")), "G1"),
                Times.Once);

            redditMock.Verify(s =>
                s.SendImageToGroup(bearerToken, It.IsAny<string>(),
                    It.Is<string>(u => u.StartsWith("data:image/png;base64,")), "G2"),
                Times.Once);

            // saved once per message
            adRepo.Verify(r => r.CreateAd(It.IsAny<Ad>()), Times.Exactly(4));
        }

        [Fact]
        public async Task SendImageToGroup_ThrowsNotSupportedException_WhenConnectionTypeMissing()
        {
            // Arrange
            var traderId = 13;
            var bearerToken = "bt";
            var bytes = Encoding.UTF8.GetBytes("x");
            var formFile = CreateMockFormFile("i.jpg", "image/jpeg", bytes);

            var dto = new DistributeAdDto
            {
                Token = traderId,
                ConnectionType = new[] { ConnectionType.Signal },
                GroupIds = new List<string> { "G1" },
                AdEntities = new CreateAdDto { Image = formFile }
            };

            var strategies = new Dictionary<ConnectionType, ISocialConnection>
            {
                { ConnectionType.WhatsApp, new Mock<ISocialConnection>().Object }
            };

            var traderRepo = new Mock<ITraderRepository>();
            traderRepo
                .Setup(r => r.GetTraderById(traderId))
                .ReturnsAsync(new Trader { Id = traderId, WhapiBearerToken = bearerToken });

            var groupRepo = new Mock<IGroupRepository>();
            groupRepo
                .Setup(r => r.GetWhitelistedGroups(traderId))
                .ReturnsAsync(new[] { new WhitelistedGroup { Id = "G1" } });

            var adRepo = new Mock<IAdRepository>();
            adRepo.Setup(r => r.CreateAd(It.IsAny<Ad>())).Returns(Task.CompletedTask);

            var service = new DistributeAdService(
                strategies,
                traderRepo.Object,
                groupRepo.Object,
                adRepo.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                service.SendImageToGroup(dto));
        }
    }
}
