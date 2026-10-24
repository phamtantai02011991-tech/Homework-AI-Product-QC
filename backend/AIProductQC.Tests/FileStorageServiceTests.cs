using System.IO;
using AIProductQC.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIProductQC.Tests;

public class FileStorageServiceTests
{
    private readonly LocalFileStorageService _storageService;

    public FileStorageServiceTests()
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        envMock.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

        var loggerMock = new Mock<ILogger<LocalFileStorageService>>();
        _storageService = new LocalFileStorageService(envMock.Object, loggerMock.Object);
    }

    [Fact]
    public void ValidateImage_ValidJpeg_ReturnsTrue()
    {
        // JPEG magic bytes: FF D8 FF E0
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 });
        var isValid = _storageService.ValidateImage(stream, "sample.jpg", stream.Length, out var error);

        Assert.True(isValid);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateImage_ValidPng_ReturnsTrue()
    {
        // PNG magic bytes: 89 50 4E 47
        var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D });
        var isValid = _storageService.ValidateImage(stream, "test_defect.png", stream.Length, out var error);

        Assert.True(isValid);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateImage_InvalidExtension_ReturnsFalse()
    {
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        var isValid = _storageService.ValidateImage(stream, "script.exe", stream.Length, out var error);

        Assert.False(isValid);
        Assert.Contains("Invalid image extension", error);
    }

    [Fact]
    public void ValidateImage_FakeJpegExtension_ReturnsFalse()
    {
        // File named .jpg but contains text/binary bytes
        var stream = new MemoryStream(new byte[] { 0x4D, 0x5A, 0x90, 0x00 }); // MZ executable header
        var isValid = _storageService.ValidateImage(stream, "virus.jpg", stream.Length, out var error);

        Assert.False(isValid);
        Assert.Contains("signature does not match", error);
    }

    [Fact]
    public void ValidateImage_ExceedingSizeLimit_ReturnsFalse()
    {
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        var hugeLength = 15L * 1024 * 1024; // 15MB
        var isValid = _storageService.ValidateImage(stream, "big.jpg", hugeLength, out var error);

        Assert.False(isValid);
        Assert.Contains("exceeds maximum allowable size", error);
    }

    [Fact]
    public void ValidateImage_EmptyFile_ReturnsFalse()
    {
        var stream = new MemoryStream();
        var isValid = _storageService.ValidateImage(stream, "empty.jpg", 0, out var error);

        Assert.False(isValid);
        Assert.Contains("empty", error);
    }
}
