using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIProductQC.Core.Interfaces;

/// <summary>
/// Service contract for secure file validation, storage, and URL resolution.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Validates and stores an uploaded image stream safely on disk.
    /// </summary>
    /// <param name="fileStream">Stream of the uploaded file.</param>
    /// <param name="originalFileName">Original file name for extension extraction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public relative URL to the saved image (e.g. /uploads/qc_{guid}.jpg).</returns>
    Task<string> SaveImageAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads stored image bytes for processing.
    /// </summary>
    /// <param name="relativeUrl">Relative URL of the image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Byte array of the image.</returns>
    Task<byte[]> GetImageBytesAsync(string relativeUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates image file extension, header signature (magic bytes), and size.
    /// </summary>
    /// <param name="fileStream">Stream of the uploaded file.</param>
    /// <param name="originalFileName">Original file name.</param>
    /// <param name="fileLength">File size in bytes.</param>
    /// <returns>True if the file is a valid image; otherwise false.</returns>
    bool ValidateImage(Stream fileStream, string originalFileName, long fileLength, out string errorMessage);
}
