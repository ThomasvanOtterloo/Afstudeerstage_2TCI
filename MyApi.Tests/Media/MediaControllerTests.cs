using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using EonWatchesAPI.Controllers;

namespace MyApi.Tests.Controllers
{
    public class MediaControllerTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly MediaController _controller;

        public MediaControllerTests()
        {
            // 1) Create a temporary directory to act as the image folder
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);

            // 2) Instantiate the controller and override its private _imageFolder field via reflection
            _controller = new MediaController();
            var field = typeof(MediaController)
                .GetField("_imageFolder", BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(_controller, _tempFolder);
        }

        [Fact]
        public void GetImage_ReturnsBadRequest_WhenFilenameIsNullOrWhitespace()
        {
            // Arrange
            string filename = "   ";

            // Act
            IActionResult result = _controller.GetImage(filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Invalid filename");
        }

        [Fact]
        public void GetImage_ReturnsBadRequest_WhenFilenameContainsTraversalOrRooted()
        {
            // Arrange
            string[] invalids = { "../secret.jpg", "C:\\evil.jpg" };

            foreach (var filename in invalids)
            {
                // Act
                IActionResult result = _controller.GetImage(filename);

                // Assert
                result.Should().BeOfType<BadRequestObjectResult>()
                      .Which.Value.Should().Be("Invalid filename");
            }
        }

        [Fact]
        public void GetImage_ReturnsBadRequest_WhenExtensionUnsupported()
        {
            // Arrange
            string filename = "image.gif"; // .gif is not allowed

            // Act
            IActionResult result = _controller.GetImage(filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Unsupported file type");
        }

        [Fact]
        public void GetImage_ReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            string filename = "nonexistent.jpg";

            // Act
            IActionResult result = _controller.GetImage(filename);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Image not found");
        }

        [Fact]
        public void GetImage_ReturnsFileStreamResult_WhenFileExists()
        {
            // Arrange
            string filename = "test.jpg";
            string filePath = Path.Combine(_tempFolder, filename);
            File.WriteAllBytes(filePath, new byte[] { 0x1, 0x2, 0x3 });

            // Act
            IActionResult result = _controller.GetImage(filename);

            // Assert
            var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
            fileResult.ContentType.Should().Be("image/jpeg");
            fileResult.EnableRangeProcessing.Should().BeTrue();

            // Verify the underlying stream can be read
            using var ms = new MemoryStream();
            fileResult.FileStream.CopyTo(ms);
            ms.ToArray().Should().Equal(new byte[] { 0x1, 0x2, 0x3 });
        }

        public void Dispose()
        {
            // Clean up the temporary folder after each test run
            try
            {
                Directory.Delete(_tempFolder, recursive: true);
            }
            catch { /* ignore cleanup errors */ }
        }
    }
}
