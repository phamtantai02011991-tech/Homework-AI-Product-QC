using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.API.DTOs;
using AIProductQC.Core.Entities;
using AIProductQC.Core.Enums;
using AIProductQC.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIProductQC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _inspectionService;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(
        IInspectionService inspectionService,
        ILogger<InspectionsController> logger)
    {
        _inspectionService = inspectionService;
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
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await _inspectionService.GetInspectionsAsync(
            productId, status, isConfirmed, startDate, endDate, search, page, pageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var dtos = items.Select(r => new InspectionReportSummaryDto(
            r.ReportId,
            r.ProductId,
            r.Product?.ProductName ?? "Unknown",
            r.InspectorId,
            r.Inspector?.InspectorName ?? "Unknown",
            r.DefectCategoryId,
            r.DefectCategory?.CategoryName ?? "Unspecified",
            r.DefectCategory?.SeverityLevel ?? SeverityLevelValues.None,
            r.ImageUrl,
            r.QualityStatus,
            r.ConfidenceScore,
            r.IsUserConfirmed,
            r.CreatedAt
        )).ToList();

        return Ok(new PaginatedListDto<InspectionReportSummaryDto>(
            totalItems,
            page,
            pageSize,
            totalPages,
            dtos
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
        var report = await _inspectionService.GetInspectionByIdAsync(id, cancellationToken);
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
    /// Invariant: Product inventory is NEVER updated by this endpoint.
    /// </summary>
    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // Enforce 10MB limit at request pipeline level (Item 10)
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    [ProducesResponseType(typeof(AnalyzeInspectionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyzeInspectionResponseDto>> AnalyzeInspection(
        [FromForm] AnalyzeInspectionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid File",
                Detail = "Please provide a valid image file for inspection."
            });
        }

        try
        {
            using var fileStream = request.File.OpenReadStream();

            var (report, isFallback, warningMessage) = await _inspectionService.AnalyzeAndCreateDraftAsync(
                request.ProductId,
                request.InspectorId,
                fileStream,
                request.File.FileName,
                request.File.Length,
                cancellationToken);

            var responseDto = new AnalyzeInspectionResponseDto(
                report.ReportId,
                report.ProductId,
                report.Product?.ProductName ?? "Unknown",
                report.InspectorId,
                report.Inspector?.InspectorName ?? "Unknown",
                report.DefectCategoryId,
                report.DefectCategory?.CategoryName ?? "No Defect",
                report.DefectCategory?.SeverityLevel ?? SeverityLevelValues.None,
                report.ImageUrl,
                report.DefectDescription,
                report.QualityStatus,
                report.ConfidenceScore,
                report.IsUserConfirmed,
                report.CreatedAt,
                isFallback,
                warningMessage
            );

            return Ok(responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
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
        try
        {
            var (report, updatedProduct, inventoryMessage) = await _inspectionService.ConfirmInspectionAsync(
                id,
                request?.QualityStatus,
                request?.DefectCategoryId,
                request?.DefectDescription,
                cancellationToken);

            var updatedInventoryDto = new ProductResponseDto(
                updatedProduct.ProductId,
                updatedProduct.ProductName,
                updatedProduct.Category,
                updatedProduct.TotalStock,
                updatedProduct.PassedStock,
                updatedProduct.FailedStock,
                Math.Max(0, updatedProduct.TotalStock - (updatedProduct.PassedStock + updatedProduct.FailedStock)),
                updatedProduct.CreatedAt
            );

            var responseDto = new ConfirmInspectionResponseDto(
                report.ReportId,
                updatedProduct.ProductId,
                updatedProduct.ProductName,
                report.QualityStatus,
                report.DefectCategory?.CategoryName ?? "No Defect",
                report.IsUserConfirmed,
                DateTime.UtcNow,
                updatedInventoryDto,
                inventoryMessage
            );

            return Ok(responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict during confirmation of report #{ReportId}", id);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Concurrency Conflict",
                Detail = "This report was updated by another session. Please refresh your data."
            });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("NeedsReview") || ex.Message.Contains("Pending"))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Unprocessable Entity",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message
            });
        }
    }
}
