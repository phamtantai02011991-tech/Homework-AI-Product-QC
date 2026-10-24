using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AIProductQC.Infrastructure.Services;

/// <summary>
/// Secure local filesystem storage service for inspection images.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly string _targetUploadDirectory;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _targetUploadDirectory = Path.Combine(webRoot, "uploads");

        if (!Directory.Exists(_targetUploadDirectory))
        {
            Directory.CreateDirectory(_targetUploadDirectory);
        }
    }

    /// <inheritdoc />
    public async Task<string> SaveImageAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            extension = ".jpg";
        }

        // Generate unique cryptographically secure GUID filename to prevent directory traversal and overwrite attacks
        var uniqueFileName = $"qc_{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_targetUploadDirectory, uniqueFileName);

        // Security check: ensure final path does not escape the upload directory
        var fullPath = Path.GetFullPath(destinationPath);
        if (!fullPath.StartsWith(Path.GetFullPath(_targetUploadDirectory), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal attempt detected.");
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        using (var fileOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(fileOutput, cancellationToken);
        }

        _logger.LogInformation("Inspection image stored securely at {Path}", fullPath);
        return $"/uploads/{uniqueFileName}";
    }

    /// <inheritdoc />
    public async Task<byte[]> GetImageBytesAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(relativeUrl);
        var filePath = Path.Combine(_targetUploadDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Image file not found: {fileName}");
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    /// <inheritdoc />
    public bool ValidateImage(Stream fileStream, string originalFileName, long fileLength, out string errorMessage)
    {
        errorMessage = string.Empty;

        // 1. Check size limit
        if (fileLength <= 0)
        {
            errorMessage = "Uploaded file is empty.";
            return false;
        }

        if (fileLength > MaxFileSizeInBytes)
        {
            errorMessage = $"Image file exceeds maximum allowable size of {MaxFileSizeInBytes / (1024 * 1024)} MB.";
            return false;
        }

        // 2. Check extension whitelist
        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            errorMessage = "Invalid image extension. Only .jpg, .jpeg, .png, and .webp formats are supported.";
            return false;
        }

        // 3. Check magic bytes (file signature verification)
        fileStream.Seek(0, SeekOrigin.Begin);
        var headerBytes = new byte[12];
        var bytesRead = fileStream.Read(headerBytes, 0, headerBytes.Length);
        fileStream.Seek(0, SeekOrigin.Begin); // Reset stream position

        if (bytesRead < 4)
        {
            errorMessage = "File header is corrupted or unreadable.";
            return false;
        }

        var isJpeg = headerBytes[0] == 0xFF && headerBytes[1] == 0xD8 && headerBytes[2] == 0xFF;
        var isPng = headerBytes[0] == 0x89 && headerBytes[1] == 0x50 && headerBytes[2] == 0x4E && headerBytes[3] == 0x47;
        var isWebp = bytesRead >= 12 &&
                     headerBytes[0] == 0x52 && headerBytes[1] == 0x49 && headerBytes[2] == 0x46 && headerBytes[3] == 0x46 &&
                     headerBytes[8] == 0x57 && headerBytes[9] == 0x45 && headerBytes[10] == 0x42 && headerBytes[11] == 0x50;

        if (!isJpeg && !isPng && !isWebp)
        {
            errorMessage = "File content signature does not match a valid JPEG, PNG, or WEBP image.";
            return false;
        }

        return true;
    }
}
