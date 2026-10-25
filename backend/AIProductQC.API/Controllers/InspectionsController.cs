using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.API.DTOs;
using AIProductQC.Core.Entities;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIProductQC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IGeminiVisionService _geminiVisionService;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IGeminiVisionService geminiVisionService,
        ILogger<InspectionsController> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _geminiVisionService = geminiVisionService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated and filterable list of inspection reports.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedListDto<InspectionReportSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<InspectionReportSummaryDto>>> GetInspections(
        [FromQuery] int? productId,
        [FromQuery] string? status,
        [FromQuery] bool? isConfirmed,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
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

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new InspectionReportSummaryDto(
                r.ReportId,
                r.ProductId,
                r.Product != null ? r.Product.ProductName : "Unknown",
                r.InspectorId,
                r.Inspector != null ? r.Inspector.InspectorName : "Unknown",
                r.DefectCategoryId,
                r.DefectCategory != null ? r.DefectCategory.CategoryName : "Unspecified",
                r.DefectCategory != null ? r.DefectCategory.SeverityLevel : "None",
                r.ImageUrl,
                r.QualityStatus,
                r.ConfidenceScore,
                r.IsUserConfirmed,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(new PaginatedListDto<InspectionReportSummaryDto>(
            totalItems,
            page,
            pageSize,
            totalPages,
            items
        ));
    }

    /// <summary>
    /// Retrieves full details of a single inspection report.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InspectionReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionReportDetailDto>> GetInspectionById(
        int id,
        CancellationToken cancellationToken)
    {
        var report = await _context.InspectionReports
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Inspector)
            .Include(r => r.DefectCategory)
            .FirstOrDefaultAsync(r => r.ReportId == id, cancellationToken);

        if (report == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Inspection report #{id} was not found."
            });
        }

        var product = report.Product!;
        var inspector = report.Inspector!;
        var category = report.DefectCategory!;

        var dto = new InspectionReportDetailDto(
            report.ReportId,
            new ProductResponseDto(
                product.ProductId,
                product.ProductName,
                product.Category,
                product.TotalStock,
                product.PassedStock,
                product.FailedStock,
                Math.Max(0, product.TotalStock - (product.PassedStock + product.FailedStock)),
                product.CreatedAt
            ),
            new InspectorResponseDto(
                inspector.InspectorId,
                inspector.InspectorName,
                inspector.Department,
                inspector.Email
            ),
            new DefectCategoryResponseDto(
                category.DefectCategoryId,
                category.CategoryName,
                category.SeverityLevel,
                category.Description
            ),
            report.ImageUrl,
            report.DefectDescription,
            report.QualityStatus,
            report.ConfidenceScore,
            report.IsUserConfirmed,
            report.CreatedAt
        );

        return Ok(dto);
    }

    /// <summary>
    /// Analyzes a product image using Gemini Vision AI and stores an unconfirmed inspection draft.
    /// IMPORTANT: Product inventory is NEVER updated by this endpoint.
    /// </summary>
    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnalyzeInspectionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyzeInspectionResponseDto>> AnalyzeInspection(
        [FromForm] AnalyzeInspectionRequestDto request,
        CancellationToken cancellationToken)
    {
        // 1. Validate Product Existence
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
        if (product == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product Not Found",
                Detail = $"Product with ID = {request.ProductId} does not exist in the system."
            });
        }

        // 2. Validate Inspector Existence
        var inspector = await _context.Inspectors.FindAsync(new object[] { request.InspectorId }, cancellationToken);
        if (inspector == null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Inspector Not Found",
                Detail = $"Inspector with ID = {request.InspectorId} is invalid."
            });
        }

        // 3. Validate Uploaded File Security and Magic Bytes
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid File",
                Detail = "Please provide a valid image file for inspection."
            });
        }

        using var fileStream = request.File.OpenReadStream();
        if (!_fileStorageService.ValidateImage(fileStream, request.File.FileName, request.File.Length, out var validationError))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Image File",
                Detail = validationError
            });
        }

        // 4. Save Image to wwwroot/uploads with unique hashed filename
        var imageUrl = await _fileStorageService.SaveImageAsync(fileStream, request.File.FileName, cancellationToken);

        // 5. Read Image Bytes for Gemini Analysis
        var imageBytes = await _fileStorageService.GetImageBytesAsync(imageUrl, cancellationToken);
        var mimeType = request.File.ContentType;

        // 6. Invoke Multimodal Gemini Vision AI
        var aiResult = await _geminiVisionService.AnalyzeProductImageAsync(
            imageBytes,
            mimeType,
            product.ProductName,
            product.Category,
            cancellationToken);

        // 7. Match AI Category to Database DefectCategory table
        var defectCategories = await _context.DefectCategories.AsNoTracking().ToListAsync(cancellationToken);
        var matchedCategory = defectCategories.FirstOrDefault(c =>
            c.CategoryName.Equals(aiResult.CategoryName, StringComparison.OrdinalIgnoreCase))
            ?? defectCategories.FirstOrDefault(c =>
                c.CategoryName.Contains(aiResult.CategoryName, StringComparison.OrdinalIgnoreCase))
            ?? defectCategories.First(c => c.DefectCategoryId == 1); // Default to No Defect (ID 1)

        // 8. Create Unconfirmed Inspection Report Entity
        var report = new InspectionReport
        {
            ProductId = product.ProductId,
            InspectorId = inspector.InspectorId,
            DefectCategoryId = matchedCategory.DefectCategoryId,
            ImageUrl = imageUrl,
            DefectDescription = aiResult.DefectDescription,
            QualityStatus = aiResult.QualityStatus,
            ConfidenceScore = aiResult.ConfidenceScore,
            IsUserConfirmed = false, // Strictly unconfirmed
            CreatedAt = DateTime.UtcNow
        };

        _context.InspectionReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved unconfirmed inspection report #{ReportId} for product {ProductId}. Inventory remains unchanged.",
            report.ReportId, product.ProductId);

        var responseDto = new AnalyzeInspectionResponseDto(
            report.ReportId,
            product.ProductId,
            product.ProductName,
            inspector.InspectorId,
            inspector.InspectorName,
            matchedCategory.DefectCategoryId,
            matchedCategory.CategoryName,
            matchedCategory.SeverityLevel,
            report.ImageUrl,
            report.DefectDescription,
            report.QualityStatus,
            report.ConfidenceScore,
            report.IsUserConfirmed,
            report.CreatedAt,
            aiResult.IsFallback,
            aiResult.ErrorMessage
        );

        return Ok(responseDto);
    }

    /// <summary>
    /// Explicitly confirms an inspection report and atomically updates product stock counters.
    /// Uses IDbContextTransaction for strict database consistency.
    /// </summary>
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(typeof(ConfirmInspectionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConfirmInspectionResponseDto>> ConfirmInspection(
        int id,
        [FromBody] ConfirmInspectionRequestDto? request,
        CancellationToken cancellationToken)
    {
        // 1. Begin Atomic EF Core Database Transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Fetch the inspection report with associated product
            var report = await _context.InspectionReports
                .Include(r => r.Product)
                .Include(r => r.DefectCategory)
                .FirstOrDefaultAsync(r => r.ReportId == id, cancellationToken);

            if (report == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"Inspection report #{id} was not found."
                });
            }

            // 3. Duplicate Confirmation Guard: prevent double-counting inventory
            if (report.IsUserConfirmed)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = $"Inspection report #{id} has already been confirmed. Repeated confirmation is rejected to prevent inventory inconsistencies."
                });
            }

            // 4. Determine final QualityStatus (allow user override or default to current report status)
            var finalStatus = !string.IsNullOrWhiteSpace(request?.QualityStatus)
                ? request.QualityStatus.Trim()
                : report.QualityStatus;

            // Normalize status casing
            if (finalStatus.Equals("Passed", StringComparison.OrdinalIgnoreCase))
            {
                finalStatus = "Passed";
            }
            else if (finalStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                finalStatus = "Failed";
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
                return UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Unprocessable Entity",
                    Detail = "Status 'NeedsReview' cannot update stock directly. Please resolve status to 'Passed' or 'Failed'."
                });
            }

            // 5. Update DefectCategory if specified by QC inspector
            if (request?.DefectCategoryId.HasValue == true)
            {
                var categoryExists = await _context.DefectCategories
                    .AnyAsync(c => c.DefectCategoryId == request.DefectCategoryId.Value, cancellationToken);

                if (!categoryExists)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid Defect Category",
                        Detail = $"Defect category #{request.DefectCategoryId.Value} does not exist."
                    });
                }

                report.DefectCategoryId = request.DefectCategoryId.Value;
            }
            else if (finalStatus == "Passed")
            {
                // Invariant: Passed report defaults to No Defect (ID: 1)
                report.DefectCategoryId = 1;
            }

            // 6. Update DefectDescription if provided
            if (!string.IsNullOrWhiteSpace(request?.DefectDescription))
            {
                report.DefectDescription = request.DefectDescription.Trim();
            }

            // 7. Atomically Update Product Stock
            var product = report.Product!;
            string inventoryMessage;

            if (finalStatus == "Passed")
            {
                product.PassedStock += 1;
                inventoryMessage = $"Report confirmed successfully. Passed stock (PassedStock) for product '{product.ProductName}' increased by 1.";
            }
            else
            {
                product.FailedStock += 1;
                inventoryMessage = $"Report confirmed successfully. Failed stock (FailedStock) for product '{product.ProductName}' increased by 1.";
            }

            // Keep TotalStock at least equal to sum of inspected stock
            product.TotalStock = Math.Max(product.TotalStock, product.PassedStock + product.FailedStock);

            // 8. Lock Report by setting IsUserConfirmed = true
            report.QualityStatus = finalStatus;
            report.IsUserConfirmed = true;

            // 9. Save Changes and Commit Transaction
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully confirmed report #{ReportId}. Product #{ProductId} stock updated: Passed={Passed}, Failed={Failed}",
                report.ReportId, product.ProductId, product.PassedStock, product.FailedStock);

            // Re-fetch category name
            var confirmedCategory = await _context.DefectCategories.FindAsync(new object[] { report.DefectCategoryId }, cancellationToken);

            var updatedInventoryDto = new ProductResponseDto(
                product.ProductId,
                product.ProductName,
                product.Category,
                product.TotalStock,
                product.PassedStock,
                product.FailedStock,
                Math.Max(0, product.TotalStock - (product.PassedStock + product.FailedStock)),
                product.CreatedAt
            );

            var responseDto = new ConfirmInspectionResponseDto(
                report.ReportId,
                product.ProductId,
                product.ProductName,
                report.QualityStatus,
                confirmedCategory?.CategoryName ?? "No Defect",
                report.IsUserConfirmed,
                DateTime.UtcNow,
                updatedInventoryDto,
                inventoryMessage
            );

            return Ok(responseDto);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Concurrency conflict during confirmation of report #{ReportId}", id);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Concurrency Conflict",
                Detail = "This report was updated by another session. Please refresh your data."
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction rolled back during confirmation of report #{ReportId}", id);
            throw;
        }
    }
}
