using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.API.DTOs;
using AIProductQC.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIProductQC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Aggregates high-level QC analytics for executive monitoring and charts.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var totalProducts = await _context.Products.CountAsync(cancellationToken);
        var totalInspections = await _context.InspectionReports.CountAsync(cancellationToken);

        var totalConfirmed = await _context.InspectionReports
            .CountAsync(r => r.IsUserConfirmed, cancellationToken);

        var totalPending = await _context.InspectionReports
            .CountAsync(r => !r.IsUserConfirmed, cancellationToken);

        var totalPassed = await _context.InspectionReports
            .CountAsync(r => r.QualityStatus == AIProductQC.Core.Enums.QualityStatusValues.Passed && r.IsUserConfirmed, cancellationToken);

        var totalFailed = await _context.InspectionReports
            .CountAsync(r => r.QualityStatus == AIProductQC.Core.Enums.QualityStatusValues.Failed && r.IsUserConfirmed, cancellationToken);

        var confirmedCount = totalPassed + totalFailed;
        var passRate = confirmedCount > 0
            ? Math.Round((double)totalPassed / confirmedCount * 100.0, 1)
            : 0.0;

        // Defect distribution for confirmed failed inspections (align with confirmed totalFailed metric)
        var defectStatsRaw = await _context.InspectionReports
            .AsNoTracking()
            .Where(r => r.QualityStatus == AIProductQC.Core.Enums.QualityStatusValues.Failed && r.IsUserConfirmed && r.DefectCategoryId != 1)
            .GroupBy(r => r.DefectCategory!.CategoryName)
            .Select(g => new
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalDefects = defectStatsRaw.Sum(d => d.Count);
        var defectDistribution = defectStatsRaw
            .Select(d => new DefectCategoryStatDto(
                d.CategoryName,
                d.Count,
                totalDefects > 0 ? Math.Round((double)d.Count / totalDefects * 100.0, 1) : 0.0
            ))
            .OrderByDescending(d => d.Count)
            .ToList();

        // Recent 8 inspections
        var recentInspections = await _context.InspectionReports
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Inspector)
            .Include(r => r.DefectCategory)
            .OrderByDescending(r => r.CreatedAt)
            .Take(8)
            .Select(r => new InspectionReportSummaryDto(
                r.ReportId,
                r.ProductId,
                r.Product != null ? r.Product.ProductName : "Không rõ",
                r.InspectorId,
                r.Inspector != null ? r.Inspector.InspectorName : "Không rõ",
                r.DefectCategoryId,
                r.DefectCategory != null ? r.DefectCategory.CategoryName : "Không xác định",
                r.DefectCategory != null ? r.DefectCategory.SeverityLevel : "None",
                r.ImageUrl,
                r.QualityStatus,
                r.ConfidenceScore,
                r.IsUserConfirmed,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto(
            totalProducts,
            totalInspections,
            totalConfirmed,
            totalPending,
            totalPassed,
            totalFailed,
            passRate,
            defectDistribution,
            recentInspections
        );

        return Ok(summary);
    }
}
