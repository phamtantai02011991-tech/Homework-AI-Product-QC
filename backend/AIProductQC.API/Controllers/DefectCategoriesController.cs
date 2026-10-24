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
[Route("api/defect-categories")]
public class DefectCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DefectCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all defect classification categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DefectCategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DefectCategoryResponseDto>>> GetDefectCategories(CancellationToken cancellationToken)
    {
        var categories = await _context.DefectCategories
            .AsNoTracking()
            .OrderBy(c => c.DefectCategoryId)
            .Select(c => new DefectCategoryResponseDto(
                c.DefectCategoryId,
                c.CategoryName,
                c.SeverityLevel,
                c.Description
            ))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }
}
