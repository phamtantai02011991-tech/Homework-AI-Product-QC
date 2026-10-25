using System;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.API.Controllers;
using AIProductQC.API.DTOs;
using AIProductQC.Core.Entities;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIProductQC.Tests;

public class ConfirmationBusinessLogicTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task ConfirmInspection_ValidPassed_IncrementsPassedStock()
    {
        using var context = CreateInMemoryDbContext();
        var fileMock = new Mock<IFileStorageService>();
        var aiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionsController>>();

        var controller = new InspectionsController(context, fileMock.Object, aiMock.Object, loggerMock.Object);

        var product = await context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        // Create an unconfirmed report
        var report = new InspectionReport
        {
            ProductId = product.ProductId,
            InspectorId = 1,
            DefectCategoryId = 1,
            ImageUrl = "/uploads/test.jpg",
            DefectDescription = "Đạt tiêu chuẩn.",
            QualityStatus = "Passed",
            ConfidenceScore = 0.95,
            IsUserConfirmed = false
        };
        context.InspectionReports.Add(report);
        await context.SaveChangesAsync();

        // Act: Confirm as Passed
        var result = await controller.ConfirmInspection(report.ReportId, new ConfirmInspectionRequestDto
        {
            QualityStatus = "Passed"
        }, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConfirmInspectionResponseDto>(okResult.Value);

        Assert.True(response.IsUserConfirmed);
        Assert.Equal("Passed", response.QualityStatus);

        var updatedProduct = await context.Products.FindAsync(1);
        Assert.NotNull(updatedProduct);
        Assert.Equal(initialPassed + 1, updatedProduct.PassedStock);
        Assert.Equal(initialFailed, updatedProduct.FailedStock);
    }

    [Fact]
    public async Task ConfirmInspection_ValidFailed_IncrementsFailedStock()
    {
        using var context = CreateInMemoryDbContext();
        var fileMock = new Mock<IFileStorageService>();
        var aiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionsController>>();

        var controller = new InspectionsController(context, fileMock.Object, aiMock.Object, loggerMock.Object);

        var product = await context.Products.FirstAsync(p => p.ProductId == 2);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        var report = new InspectionReport
        {
            ProductId = product.ProductId,
            InspectorId = 1,
            DefectCategoryId = 2,
            ImageUrl = "/uploads/test_scratch.jpg",
            DefectDescription = "Trầy xước cạnh.",
            QualityStatus = "Failed",
            ConfidenceScore = 0.92,
            IsUserConfirmed = false
        };
        context.InspectionReports.Add(report);
        await context.SaveChangesAsync();

        // Act: Confirm as Failed
        var result = await controller.ConfirmInspection(report.ReportId, new ConfirmInspectionRequestDto
        {
            QualityStatus = "Failed",
            DefectCategoryId = 2
        }, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConfirmInspectionResponseDto>(okResult.Value);

        Assert.True(response.IsUserConfirmed);
        Assert.Equal("Failed", response.QualityStatus);

        var updatedProduct = await context.Products.FindAsync(2);
        Assert.NotNull(updatedProduct);
        Assert.Equal(initialFailed + 1, updatedProduct.FailedStock);
        Assert.Equal(initialPassed, updatedProduct.PassedStock);
    }

    [Fact]
    public async Task ConfirmInspection_AlreadyConfirmed_ReturnsConflict()
    {
        using var context = CreateInMemoryDbContext();
        var fileMock = new Mock<IFileStorageService>();
        var aiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionsController>>();

        var controller = new InspectionsController(context, fileMock.Object, aiMock.Object, loggerMock.Object);

        var report = new InspectionReport
        {
            ProductId = 1,
            InspectorId = 1,
            DefectCategoryId = 1,
            ImageUrl = "/uploads/test.jpg",
            DefectDescription = "Đã xác nhận trước đó.",
            QualityStatus = "Passed",
            ConfidenceScore = 0.99,
            IsUserConfirmed = true // ALREADY CONFIRMED
        };
        context.InspectionReports.Add(report);
        await context.SaveChangesAsync();

        // Act: Repeat confirmation
        var result = await controller.ConfirmInspection(report.ReportId, new ConfirmInspectionRequestDto(), CancellationToken.None);

        // Assert: 409 Conflict
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Contains("already been confirmed", problem.Detail);
    }

    [Fact]
    public async Task ConfirmInspection_WithNeedsReviewStatus_ReturnsUnprocessableEntity()
    {
        using var context = CreateInMemoryDbContext();
        var fileMock = new Mock<IFileStorageService>();
        var aiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionsController>>();

        var controller = new InspectionsController(context, fileMock.Object, aiMock.Object, loggerMock.Object);

        var report = new InspectionReport
        {
            ProductId = 1,
            InspectorId = 1,
            DefectCategoryId = 1,
            ImageUrl = "/uploads/test_blurry.jpg",
            DefectDescription = "Ảnh mờ.",
            QualityStatus = "NeedsReview",
            ConfidenceScore = 0.40,
            IsUserConfirmed = false
        };
        context.InspectionReports.Add(report);
        await context.SaveChangesAsync();

        // Act: Attempt to confirm without resolving to Passed or Failed
        var result = await controller.ConfirmInspection(report.ReportId, new ConfirmInspectionRequestDto
        {
            QualityStatus = "NeedsReview"
        }, CancellationToken.None);

        // Assert: 422 Unprocessable Entity
        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(unprocessableResult.Value);
        Assert.Contains("NeedsReview", problem.Detail);
    }
}
