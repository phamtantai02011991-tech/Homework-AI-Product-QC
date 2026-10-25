using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.Core.Entities;
using AIProductQC.Core.Enums;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIProductQC.Infrastructure.Services;

/// <summary>
/// Business service coordinating quality inspections, AI defect classification,
/// and atomic stock transactions.
/// </summary>
public class InspectionService : IInspectionService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IGeminiVisionService _geminiVisionService;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IGeminiVisionService geminiVisionService,
        ILogger<InspectionService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _geminiVisionService = geminiVisionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(InspectionReport Report, bool IsFallback, string? WarningMessage)> AnalyzeAndCreateDraftAsync(
        int productId,
        int inspectorId,
        Stream fileStream,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken = default)
    {
        // 1. Verify Product existence
        var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID = {productId} does not exist in the system.");
        }

        // 2. Verify Inspector existence
        var inspector = await _context.Inspectors.FindAsync(new object[] { inspectorId }, cancellationToken);
        if (inspector == null)
        {
            throw new ArgumentException($"Inspector with ID = {inspectorId} does not exist.");
        }

        // 3. Validate image readability and deduce canonical MIME type directly from magic bytes
        if (!_fileStorageService.ValidateImage(fileStream, fileName, fileLength, out var detectedMimeType, out var validationError))
        {
            throw new ArgumentException(validationError);
        }

        // 4. Save image securely to disk
        var imageUrl = await _fileStorageService.SaveImageAsync(fileStream, fileName, cancellationToken);
        var imageBytes = await _fileStorageService.GetImageBytesAsync(imageUrl, cancellationToken);

        // 5. Invoke Gemini Vision AI using canonical verified MIME type
        var aiResult = await _geminiVisionService.AnalyzeProductImageAsync(
            imageBytes,
            detectedMimeType,
            product.ProductName,
            product.Category,
            cancellationToken);

        // 6. Dynamically resolve "No Defect" category from database rather than hardcoding ID
        var defectCategories = await _context.DefectCategories.AsNoTracking().ToListAsync(cancellationToken);
        var defaultNoDefectCategory = defectCategories.FirstOrDefault(c =>
            c.CategoryName.Contains("Không có lỗi", StringComparison.OrdinalIgnoreCase) ||
            c.SeverityLevel.Equals(SeverityLevelValues.None, StringComparison.OrdinalIgnoreCase))
            ?? defectCategories.First();

        var matchedCategory = defectCategories.FirstOrDefault(c =>
            c.CategoryName.Equals(aiResult.CategoryName, StringComparison.OrdinalIgnoreCase))
            ?? defectCategories.FirstOrDefault(c =>
                c.CategoryName.Contains(aiResult.CategoryName, StringComparison.OrdinalIgnoreCase))
            ?? defaultNoDefectCategory;

        // 7. Persist unconfirmed inspection draft (Zero Inventory Mutation Guarantee)
        var report = new InspectionReport
        {
            ProductId = product.ProductId,
            InspectorId = inspector.InspectorId,
            DefectCategoryId = matchedCategory.DefectCategoryId,
            ImageUrl = imageUrl,
            DefectDescription = aiResult.DefectDescription,
            QualityStatus = aiResult.QualityStatus,
            ConfidenceScore = aiResult.ConfidenceScore,
            IsUserConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.InspectionReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        // Populate navigation properties for response mapping
        report.Product = product;
        report.Inspector = inspector;
        report.DefectCategory = matchedCategory;

        _logger.LogInformation("Draft inspection report #{ReportId} created for product #{ProductId}. Stock remains untouched.",
            report.ReportId, product.ProductId);

        return (report, aiResult.IsFallback, aiResult.ErrorMessage);
    }

    /// <inheritdoc />
    public async Task<(InspectionReport Report, Product UpdatedProduct, string InventoryMessage)> ConfirmInspectionAsync(
        int reportId,
        string? requestedQualityStatus,
        int? requestedCategoryId,
        string? customDescription,
        CancellationToken cancellationToken = default)
    {
        // 1. Begin atomic EF Core database transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var report = await _context.InspectionReports
                .Include(r => r.Product)
                .Include(r => r.DefectCategory)
                .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);

            if (report == null)
            {
                throw new KeyNotFoundException($"Inspection report #{reportId} was not found.");
            }

            // 2. Duplicate confirmation guard
            if (report.IsUserConfirmed)
            {
                throw new InvalidOperationException(
                    $"Inspection report #{reportId} has already been confirmed. Repeated confirmation is rejected to prevent inventory inconsistencies.");
            }

            // 3. Resolve and validate target status using QualityStatus enum
            var targetStatusString = !string.IsNullOrWhiteSpace(requestedQualityStatus)
                ? requestedQualityStatus.Trim()
                : report.QualityStatus;

            if (!QualityStatusValues.TryParse(targetStatusString, out var resolvedStatus) ||
                resolvedStatus == QualityStatus.NeedsReview ||
                resolvedStatus == QualityStatus.Pending)
            {
                throw new ArgumentException(
                    "Status 'NeedsReview' or 'Pending' cannot update stock directly. Please resolve status to 'Passed' or 'Failed'.");
            }

            var finalStatus = resolvedStatus == QualityStatus.Passed
                ? QualityStatusValues.Passed
                : QualityStatusValues.Failed;

            // 4. Update Defect Category dynamically
            if (requestedCategoryId.HasValue)
            {
                var categoryExists = await _context.DefectCategories
                    .AnyAsync(c => c.DefectCategoryId == requestedCategoryId.Value, cancellationToken);

                if (!categoryExists)
                {
                    throw new ArgumentException($"Defect category #{requestedCategoryId.Value} does not exist.");
                }

                report.DefectCategoryId = requestedCategoryId.Value;
            }
            else if (finalStatus == QualityStatusValues.Passed)
            {
                var defaultNoDefect = await _context.DefectCategories
                    .FirstOrDefaultAsync(c =>
                        c.CategoryName.Contains("Không có lỗi") ||
                        c.SeverityLevel == SeverityLevelValues.None, cancellationToken);

                if (defaultNoDefect != null)
                {
                    report.DefectCategoryId = defaultNoDefect.DefectCategoryId;
                }
            }

            if (!string.IsNullOrWhiteSpace(customDescription))
            {
                report.DefectDescription = customDescription.Trim();
            }

            // 5. Atomically update inventory counters
            var product = report.Product!;
            string inventoryMessage;

            if (finalStatus == QualityStatusValues.Passed)
            {
                product.PassedStock += 1;
                inventoryMessage = $"Report confirmed successfully. Passed stock (PassedStock) for product '{product.ProductName}' increased by 1.";
            }
            else
            {
                product.FailedStock += 1;
                inventoryMessage = $"Report confirmed successfully. Failed stock (FailedStock) for product '{product.ProductName}' increased by 1.";
            }

            product.TotalStock = Math.Max(product.TotalStock, product.PassedStock + product.FailedStock);

            // 6. Lock report
            report.QualityStatus = finalStatus;
            report.IsUserConfirmed = true;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully confirmed report #{ReportId}. Stock updated: Passed={Passed}, Failed={Failed}",
                report.ReportId, product.PassedStock, product.FailedStock);

            return (report, product, inventoryMessage);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Transaction rollback completed or aborted.");
            }
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<InspectionReport> Items, int TotalCount)> GetInspectionsAsync(
        int? productId,
        string? status,
        bool? isConfirmed,
        DateTime? startDate,
        DateTime? endDate,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.InspectionReports
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Inspector)
            .Include(r => r.DefectCategory)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmedStatus = status.Trim();
            query = query.Where(r => r.QualityStatus == trimmedStatus);
        }

        if (isConfirmed.HasValue)
        {
            query = query.Where(r => r.IsUserConfirmed == isConfirmed.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                (r.Product != null && r.Product.ProductName.Contains(term)) ||
                (r.Inspector != null && r.Inspector.InspectorName.Contains(term)) ||
                (r.DefectCategory != null && r.DefectCategory.CategoryName.Contains(term)) ||
                r.DefectDescription.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<InspectionReport?> GetInspectionByIdAsync(int reportId, CancellationToken cancellationToken = default)
    {
        return await _context.InspectionReports
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Inspector)
            .Include(r => r.DefectCategory)
            .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);
    }
}
