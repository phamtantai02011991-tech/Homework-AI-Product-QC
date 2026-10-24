using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.API.DTOs;
using AIProductQC.Core.Entities;
using AIProductQC.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIProductQC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all products with their current stock and inspection totals.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.ProductName.Contains(term) || p.Category.Contains(term));
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponseDto(
                p.ProductId,
                p.ProductName,
                p.Category,
                p.TotalStock,
                p.PassedStock,
                p.FailedStock,
                Math.Max(0, p.TotalStock - (p.PassedStock + p.FailedStock)),
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    /// <summary>
    /// Retrieves single product details and its inspection pass rate.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailResponseDto>> GetProductById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.InspectionReports)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Product with ID = {id} was not found."
            });
        }

        var totalInspected = product.PassedStock + product.FailedStock;
        var passRate = totalInspected > 0
            ? Math.Round((double)product.PassedStock / totalInspected * 100.0, 1)
            : 100.0;

        var dto = new ProductDetailResponseDto(
            product.ProductId,
            product.ProductName,
            product.Category,
            product.TotalStock,
            product.PassedStock,
            product.FailedStock,
            Math.Max(0, product.TotalStock - totalInspected),
            passRate,
            product.CreatedAt,
            product.InspectionReports.Count
        );

        return Ok(dto);
    }

    /// <summary>
    /// Registers a new product batch into inventory.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(
        [FromBody] CreateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            Category = request.Category.Trim(),
            TotalStock = Math.Max(0, request.TotalStock),
            PassedStock = 0,
            FailedStock = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var responseDto = new ProductResponseDto(
            product.ProductId,
            product.ProductName,
            product.Category,
            product.TotalStock,
            product.PassedStock,
            product.FailedStock,
            product.TotalStock,
            product.CreatedAt
        );

        return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, responseDto);
    }
}
