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
public class InspectorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InspectorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all authorized QC inspectors for selection.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InspectorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InspectorResponseDto>>> GetInspectors(CancellationToken cancellationToken)
    {
        var inspectors = await _context.Inspectors
            .AsNoTracking()
            .OrderBy(i => i.InspectorName)
            .Select(i => new InspectorResponseDto(
                i.InspectorId,
                i.InspectorName,
                i.Department,
                i.Email
            ))
            .ToListAsync(cancellationToken);

        return Ok(inspectors);
    }
}
