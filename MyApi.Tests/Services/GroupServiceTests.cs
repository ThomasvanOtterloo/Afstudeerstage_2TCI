﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using EonWatchesAPI.DbContext;
using EonWatchesAPI.DbContext.I_Repositories;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Factories.SocialPlatforms;
using EonWatchesAPI.Services.Services;
using EonWatchesAPI.Factories;

namespace MyApi.Tests.Services
{
    public class GroupServiceTests
    {
        private readonly Mock<ISocialConnection> _socialConnectionMock = new();
        private readonly Mock<IGroupRepository> _groupRepoMock = new();
        private readonly GroupService _service;

        public GroupServiceTests()
        {
            _service = new GroupService(_socialConnectionMock.Object, _groupRepoMock.Object);
        }

        [Fact]
        public async Task GetGroups_ReturnsListFromSocialConnection()
        {
            // Arrange
            var bearerToken = "jwt-token";
            var groups = new List<GroupDto>
            {
                new GroupDto { Id = "G1", GroupName = "Group One" },
                new GroupDto { Id = "G2", GroupName = "Group Two" }
            };

            _socialConnectionMock
                .Setup(s => s.GetGroupsByUser(bearerToken))
                .ReturnsAsync(groups);

            // Act
            var result = await _service.GetGroups(bearerToken);

            // Assert
            result.Should().BeEquivalentTo(groups);
            _socialConnectionMock.Verify(s => s.GetGroupsByUser(bearerToken), Times.Once);
        }

        [Fact]
        public async Task GetWhitelistedGroups_ReturnsListFromRepository()
        {
            // Arrange
            int traderId = 5;
            var whitelisted = new List<WhitelistedGroup>
            {
                new WhitelistedGroup { Id = "W1", GroupName = "WhiteOne" },
                new WhitelistedGroup { Id = "W2", GroupName = "WhiteTwo" }
            };

            _groupRepoMock
                .Setup(r => r.GetWhitelistedGroups(traderId))
                .ReturnsAsync(whitelisted);

            // Act
            var result = await _service.GetWhitelistedGroups(traderId);

            // Assert
            result.Should().BeEquivalentTo(whitelisted);
            _groupRepoMock.Verify(r => r.GetWhitelistedGroups(traderId), Times.Once);
        }

        [Fact]
        public async Task DeleteWhitelistedGroup_ForwardsTraderIdToRepository()
        {
            // Arrange
            int traderId = 2;
            string groupId = "G1";

            _groupRepoMock
                .Setup(r => r.DeleteWhitelistedGroup(traderId, groupId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteWhitelistedGroup(traderId, groupId);

            // Assert: verify that the same trader id was used
            _groupRepoMock.Verify(r => r.DeleteWhitelistedGroup(traderId, groupId), Times.Once);
        }

        [Fact]
        public async Task WhitelistGroup_ForwardsTraderIdToRepository()
        {
            // Arrange
            int traderId = 99;
            string groupId = "G3";
            string groupName = "GroupThree";

            _groupRepoMock
                .Setup(r => r.WhitelistGroup(traderId, groupId, groupName))
                .Returns(Task.CompletedTask);

            // Act
            await _service.WhitelistGroup(traderId, groupId, groupName);

            // Assert
            _groupRepoMock.Verify(r => r.WhitelistGroup(traderId, groupId, groupName), Times.Once);
        }
    }
}
