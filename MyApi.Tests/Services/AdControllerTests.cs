using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using EonWatchesAPI.Controllers;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Services.Services;

namespace MyApi.Tests.Controllers
{
    public class AdControllerTests
    {
        private readonly Mock<IAdService> _adServiceMock = new();
        private readonly AdController _controller;

        public AdControllerTests()
        {
            _controller = new AdController(_adServiceMock.Object);
        }

        // ------------------------------------------------------------------------------------
        // 1) GetAds_NoFilter_ReturnsAllAds → 200 OK + full list
        // ------------------------------------------------------------------------------------
        [Fact]
        public async Task GetAds_NoFilter_ReturnsAllAds()
        {
            // Arrange
            var allAds = new List<Ad>
            {
                new Ad { Id = 1, Brand = "Omega", Model = "Seamaster", ReferenceNumber = "REF001", Price = 2500m },
                new Ad { Id = 2, Brand = "Rolex", Model = "Submariner", ReferenceNumber = "REF002", Price = 8000m },
                new Ad { Id = 3, Brand = "Tudor", Model = "Black Bay", ReferenceNumber = "REF003", Price = 3500m }
            };

            var filter = new AdFilterDto
            {
                Brand = null,
                Model = null,
                ReferenceNumber = null,
                DaysAgo = null
            };

            // Mock IAdService so that GetAdsByFilter(filter) returns allAds
            _adServiceMock
                .Setup(s => s.GetAdsByFilter(filter))
                .ReturnsAsync(allAds);

            // Act
            IActionResult actionResult = await _controller.GetAds(filter);

            // Assert: 
            //  a) We expect OkObjectResult (HTTP 200)
            //  b) Its Value should be equivalent to allAds
            actionResult
                .Should()
                .BeOfType<OkObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(allAds);

            _adServiceMock.Verify(s => s.GetAdsByFilter(filter), Times.Once);
        }

        // ------------------------------------------------------------------------------------
        // 2) GetAds_FilterByReferenceNumber_ReturnsMatchingAds → 200 OK + filtered list
        // ------------------------------------------------------------------------------------
        [Fact]
        public async Task GetAds_FilterByReferenceNumber_ReturnsMatchingAds()
        {
            // Arrange
            var matchingAds = new List<Ad>
            {
                new Ad { Id = 4, Brand = "Omega", Model = "Speedmaster", ReferenceNumber = "REF123", Price = 5000m },
                new Ad { Id = 5, Brand = "Rolex", Model = "Datejust",    ReferenceNumber = "REF123", Price = 7000m }
            };

            var filter = new AdFilterDto
            {
                Brand = null,
                Model = null,
                ReferenceNumber = "REF123",
                DaysAgo = null
            };

            _adServiceMock
                .Setup(s => s.GetAdsByFilter(filter))
                .ReturnsAsync(matchingAds);

            // Act
            IActionResult actionResult = await _controller.GetAds(filter);

            // Assert
            actionResult
                .Should()
                .BeOfType<OkObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(matchingAds);

            _adServiceMock.Verify(s => s.GetAdsByFilter(filter), Times.Once);
        }

        // ------------------------------------------------------------------------------------
        // 3) GetAds_FilterByBrandAndModel_ReturnsMatchingAds → 200 OK + filtered list
        // ------------------------------------------------------------------------------------
        [Fact]
        public async Task GetAds_FilterByBrandAndModel_ReturnsMatchingAds()
        {
            // Arrange
            var matchingAds = new List<Ad>
            {
                new Ad { Id = 6, Brand = "Omega", Model = "Seamaster", ReferenceNumber = "REF010", Price = 2600m },
                new Ad { Id = 7, Brand = "Omega", Model = "Seamaster", ReferenceNumber = "REF011", Price = 2700m }
            };

            var filter = new AdFilterDto
            {
                Brand = "Omega",
                Model = "Seamaster",
                ReferenceNumber = null,
                DaysAgo = null
            };

            _adServiceMock
                .Setup(s => s.GetAdsByFilter(filter))
                .ReturnsAsync(matchingAds);

            // Act
            IActionResult actionResult = await _controller.GetAds(filter);

            // Assert
            actionResult
                .Should()
                .BeOfType<OkObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(matchingAds);

            _adServiceMock.Verify(s => s.GetAdsByFilter(filter), Times.Once);
        }

        // ------------------------------------------------------------------------------------
        // 4) GetAds_FilterByBrandAndReferenceNumber_ReturnsMatchingAds → 200 OK + filtered list
        // ------------------------------------------------------------------------------------
        [Fact]
        public async Task GetAds_FilterByBrandAndReferenceNumber_ReturnsMatchingAds()
        {
            // Arrange
            var matchingAds = new List<Ad>
            {
                new Ad { Id = 8, Brand = "Rolex", Model = "Explorer", ReferenceNumber = "REF500", Price = 6500m },
                new Ad { Id = 9, Brand = "Rolex", Model = "Oyster",    ReferenceNumber = "REF500", Price = 6200m }
            };

            var filter = new AdFilterDto
            {
                Brand = "Rolex",
                Model = null,
                ReferenceNumber = "REF500",
                DaysAgo = null
            };

            _adServiceMock
                .Setup(s => s.GetAdsByFilter(filter))
                .ReturnsAsync(matchingAds);

            // Act
            IActionResult actionResult = await _controller.GetAds(filter);

            // Assert
            actionResult
                .Should()
                .BeOfType<OkObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(matchingAds);

            _adServiceMock.Verify(s => s.GetAdsByFilter(filter), Times.Once);
        }
    

    [Fact]
        public async Task GetAdById_ShouldReturnAd_WhenExists()
        {
            // Arrange
            var expectedAd = new Ad { Id = 42, Brand = "Test ad" };
            _adServiceMock
                .Setup(s => s.GetAdById(42))
                .ReturnsAsync(expectedAd);

            // Act
            IActionResult actionResult = await _controller.GetAdById(42);

            // Assert: 
            //  a) We expect OkObjectResult (HTTP 200)
            //  b) The Value inside that OkObjectResult is equivalent to expectedAd
            actionResult
                .Should()
                .BeOfType<OkObjectResult>()
                .Which.Value
                .Should()
                .BeEquivalentTo(expectedAd);

            _adServiceMock.Verify(s => s.GetAdById(42), Times.Once);
        }


        [Fact]
        public async Task GetAdById_ShouldReturnNoContent_WhenNotExists()
        {
            // Arrange
            _adServiceMock
                .Setup(s => s.GetAdById(42))
                .ReturnsAsync((Ad)null!);

            // Act
            IActionResult actionResult = await _controller.GetAdById(42);

            // Assert: it should be a NoContentResult (HTTP 204)
            actionResult
                .Should()
                .BeOfType<NoContentResult>();

            _adServiceMock.Verify(s => s.GetAdById(42), Times.Once);
        }


        [Fact]
        public async Task GetAdsByFilter_DaysAgo4_ReturnsAdCreated3DaysAgo()
        {
            // Arrange
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
            var recentAds = new List<Ad>
            {
                new Ad { Id = 1, Brand = "Test", Model = "X", ReferenceNumber = "REF1", Price = 100m, CreatedAt = threeDaysAgo }
            };

            // Mock repository to return that one ad regardless of parameters
            var repoMock = new Mock<IAdRepository>();
            repoMock
                .Setup(r => r.GetAdsFiltered(
                    /* Brand: */          null,
                    /* Model: */          null,
                    /* ReferenceNumber: */ null,
                    /* DaysAgo: */        4))
                .ReturnsAsync(recentAds);

            var service = new AdService(repoMock.Object);

            var filter = new AdFilterDto
            {
                Brand = null,
                Model = null,
                ReferenceNumber = null,
                DaysAgo = 4
            };

            // Act
            var result = await service.GetAdsByFilter(filter);

            // Assert: the ad created 3 days ago should be returned when DaysAgo = 4
            result.Should().ContainSingle()
                  .Which.CreatedAt.Should().BeCloseTo(threeDaysAgo, precision: TimeSpan.FromSeconds(1));

            repoMock.Verify(r => r.GetAdsFiltered(null, null, null, 4), Times.Once);
        }

        [Fact]
        public async Task GetAdsByFilter_DaysAgo2_ReturnsNoAds_IfCreated3DaysAgo()
        {
            // Arrange
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
            var olderAds = new List<Ad>
    {
        new Ad { Id = 2, Brand = "Test", Model = "Y", ReferenceNumber = "REF2", Price = 200m, CreatedAt = threeDaysAgo }
    };

            // Mock repository: when DaysAgo = 2, it should receive that parameter and return an empty list
            var repoMock = new Mock<IAdRepository>();
            repoMock
                .Setup(r => r.GetAdsFiltered(
                    /* Brand: */          null,
                    /* Model: */          null,
                    /* ReferenceNumber: */ null,
                    /* DaysAgo: */        2))
                .ReturnsAsync(new List<Ad>());

            var service = new AdService(repoMock.Object);

            var filter = new AdFilterDto
            {
                Brand = null,
                Model = null,
                ReferenceNumber = null,
                DaysAgo = 2
            };

            // Act
            var result = await service.GetAdsByFilter(filter);

            // Assert: because the only ad was created 3 days ago, and DaysAgo = 2, no ads should be returned
            result.Should().BeEmpty();

            repoMock.Verify(r => r.GetAdsFiltered(null, null, null, 2), Times.Once);
        }


    }
}
