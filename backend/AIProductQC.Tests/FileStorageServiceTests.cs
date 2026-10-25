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
        // JPEG magic bytes (FF D8 FF E0 ...) and EOI (FF D9)
        var bytes = new byte[] { 
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 
            0x01, 0x01, 0x00, 0x60, 0x00, 0x60, 0x00, 0x00, 0xFF, 0xD9 
        };
        var stream = new MemoryStream(bytes);
        var isValid = _storageService.ValidateImage(stream, "sample.jpg", stream.Length, out var detectedMime, out var error);

        Assert.True(isValid);
        Assert.Equal("image/jpeg", detectedMime);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateImage_TruncatedJpeg_MissingEoi_ReturnsFalse()
    {
        // JPEG header without closing FF D9
        var bytes = new byte[] { 
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 
            0x01, 0x01, 0x00, 0x60, 0x00, 0x60, 0x00, 0x00 
        };
        var stream = new MemoryStream(bytes);
        var isValid = _storageService.ValidateImage(stream, "truncated.jpg", stream.Length, out var error);

        Assert.False(isValid);
        Assert.Contains("missing End-of-Image", error);
    }

    [Fact]
    public void ValidateImage_ValidPng_ReturnsTrue()
    {
        // PNG magic bytes + chunks with closing IEND
        var bytes = new byte[] { 
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 
            0x89, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 
            0x44, 0xAE, 0x42, 0x60, 0x82 
        };
        var stream = new MemoryStream(bytes);
        var isValid = _storageService.ValidateImage(stream, "test_defect.png", stream.Length, out var detectedMime, out var error);

        Assert.True(isValid);
        Assert.Equal("image/png", detectedMime);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateImage_TruncatedPng_MissingIend_ReturnsFalse()
    {
        // PNG header without IEND footer
        var bytes = new byte[] { 
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89 
        };
        var stream = new MemoryStream(bytes);
        var isValid = _storageService.ValidateImage(stream, "truncated.png", stream.Length, out var error);

        Assert.False(isValid);
        Assert.Contains("missing valid IEND", error);
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
