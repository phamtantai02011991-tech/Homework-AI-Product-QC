using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.Core.Entities;

namespace AIProductQC.Core.Interfaces;

/// <summary>
/// Domain service contract orchestrating product quality inspections,
/// AI vision analysis, unconfirmed draft management, and atomic confirmation transactions.
/// </summary>
public interface IInspectionService
{
    /// <summary>
    /// Validates an image, executes Gemini Vision AI analysis, and persists an unconfirmed inspection draft.
    /// Invariant: Inventory is NEVER modified during this analysis phase.
    /// </summary>
    Task<(InspectionReport Report, bool IsFallback, string? WarningMessage)> AnalyzeAndCreateDraftAsync(
        int productId,
        int inspectorId,
        Stream fileStream,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly confirms an inspection report, atomically updates inventory stock inside an IDbContextTransaction,
    /// and locks the report against duplicate confirmations.
    /// </summary>
    Task<(InspectionReport Report, Product UpdatedProduct, string InventoryMessage)> ConfirmInspectionAsync(
        int reportId,
        string? requestedQualityStatus,
        int? requestedCategoryId,
        string? customDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated and filtered collection of inspection reports.
    /// </summary>
    Task<(IReadOnlyList<InspectionReport> Items, int TotalCount)> GetInspectionsAsync(
        int? productId,
        string? status,
        bool? isConfirmed,
        DateTime? startDate,
        DateTime? endDate,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves full details of a specific inspection report by identifier.
    /// </summary>
    Task<InspectionReport?> GetInspectionByIdAsync(
        int reportId,
        CancellationToken cancellationToken = default);
}
