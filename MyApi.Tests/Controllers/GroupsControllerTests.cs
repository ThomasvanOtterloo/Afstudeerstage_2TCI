using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using EonWatchesAPI.Controllers;
using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using EonWatchesAPI.DbContext;

namespace MyApi.Tests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly Mock<IGroupService> _groupServiceMock = new();
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            _controller = new GroupsController(_groupServiceMock.Object);
        }

        [Fact]
        public async Task GetUserGroups_ReturnsOkWithList_WhenServiceSucceeds()
        {
            // Arrange
            var token = "valid-token";
            var groups = new List<GroupDto>
            {
                new GroupDto { Id = "G1", GroupName = "Group One" },
                new GroupDto { Id = "G2", GroupName = "Group Two" }
            };
            _groupServiceMock
                .Setup(s => s.GetGroups(token))
                .ReturnsAsync(groups);

            // Act
            ActionResult<List<GroupDto>> actionResult = await _controller.GetUserGroups(token);

            // Assert: should be 200 OK with the list
            actionResult.Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Which.Value
                .Should()
                .BeEquivalentTo(groups);

            _groupServiceMock.Verify(s => s.GetGroups(token), Times.Once);
        }

        [Fact]
        public async Task GetUserGroups_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var token = "bad-token";
            var exceptionMessage = "Invalid token";
            _groupServiceMock
                .Setup(s => s.GetGroups(token))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            ActionResult<List<GroupDto>> actionResult = await _controller.GetUserGroups(token);

            // Assert: should be 400 Bad Request with the exception message prefix
            actionResult.Result
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be("GetUserGroups triggered een exception: " + exceptionMessage);

            _groupServiceMock.Verify(s => s.GetGroups(token), Times.Once);
        }

        [Fact]
        public async Task GetWhitelistedGroups_ReturnsOkWithList_WhenServiceSucceeds()
        {
            // Arrange
            var traderId = 123;
            var entityList = new List<WhitelistedGroup>
                {
                    new WhitelistedGroup { Id = "W1", GroupName = "Whitelist One", TraderId = traderId }
                };

            // Note: service returns List<WhitelistedGroups>, not List<GroupDto>
            _groupServiceMock
                .Setup(s => s.GetWhitelistedGroups(traderId))
                .ReturnsAsync(entityList);

            // Act:
            ActionResult<List<GroupDto>> actionResult = await _controller.GetWhitelistedGroups(traderId);

            // Assert: should be 200 OK with the list of WhitelistedGroups entities
            actionResult.Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Which.Value
                .Should()
                .BeEquivalentTo(entityList);

            _groupServiceMock.Verify(s => s.GetWhitelistedGroups(traderId), Times.Once);
        }


        [Fact]
        public async Task GetWhitelistedGroups_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var traderId = 999;
            var exceptionMessage = "Database error";
            _groupServiceMock
                .Setup(s => s.GetWhitelistedGroups(traderId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            ActionResult<List<GroupDto>> actionResult = await _controller.GetWhitelistedGroups(traderId);

            // Assert: should be 400 Bad Request with the exception message
            actionResult.Result
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _groupServiceMock.Verify(s => s.GetWhitelistedGroups(traderId), Times.Once);
        }

        [Fact]
        public async Task WhitelistGroupId_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var traderId = 55;
            var groupId = "G100";
            var groupName = "NewGroup";
            _groupServiceMock
                .Setup(s => s.WhitelistGroup(traderId, groupId, groupName))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.WhitelistGroupId(traderId, groupId, groupName);

            // Assert: should be 200 OK
            actionResult.Should().BeOfType<OkResult>();
            _groupServiceMock.Verify(s => s.WhitelistGroup(traderId, groupId, groupName), Times.Once);
        }

        [Fact]
        public async Task WhitelistGroupId_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var traderId = 66;
            var groupId = "G200";
            var groupName = "BadGroup";
            var exceptionMessage = "Cannot whitelist";
            _groupServiceMock
                .Setup(s => s.WhitelistGroup(traderId, groupId, groupName))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.WhitelistGroupId(traderId, groupId, groupName);

            // Assert: should be 400 Bad Request with the exception message
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                .Should()
                .Be(exceptionMessage);

            _groupServiceMock.Verify(s => s.WhitelistGroup(traderId, groupId, groupName), Times.Once);
        }

        [Fact]
        public async Task DeWhitelistGroupId_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var traderId = "42";
            var groupId = "G300";
            _groupServiceMock
                .Setup(s => s.DeleteWhitelistedGroup(traderId, groupId))
                .Returns(Task.CompletedTask);

            // Act
            IActionResult actionResult = await _controller.DeWhitelistGroupId(traderId, groupId);

            // Assert: should be 200 OK
            actionResult.Should().BeOfType<OkResult>();
            _groupServiceMock.Verify(s => s.DeleteWhitelistedGroup(traderId, groupId), Times.Once);
        }

        [Fact]
        public async Task DeWhitelistGroupId_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var traderId = "84";
            var groupId = "G400";
            var exceptionMessage = "Cannot delete";
            _groupServiceMock
                .Setup(s => s.DeleteWhitelistedGroup(traderId, groupId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            IActionResult actionResult = await _controller.DeWhitelistGroupId(traderId, groupId);

            // Assert: should be 400 Bad Request with the exception message
            actionResult
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Which.Value
                 .Should()
                 .Be(exceptionMessage);

            _groupServiceMock.Verify(s => s.DeleteWhitelistedGroup(traderId, groupId), Times.Once);
        }
    }
}
