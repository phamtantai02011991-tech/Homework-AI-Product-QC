using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIProductQC.Infrastructure.Services;

/// <summary>
/// Google Gemini Vision Multimodal AI inspection implementation.
/// </summary>
public class GeminiVisionService : IGeminiVisionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiVisionService> _logger;

    public GeminiVisionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiVisionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiAnalysisResult> AnalyzeProductImageAsync(
        byte[] imageBytes,
        string mimeType,
        string? productName = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        // Retrieve API key securely from user secrets, configuration, or environment variables
        var apiKey = _configuration["GEMINI_API_KEY"]
            ?? _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Returning offline manual inspection fallback.");
            return new AiAnalysisResult(
                CategoryName: "Không có lỗi (No Defect)",
                DefectDescription: "Khóa API Google Gemini (GEMINI_API_KEY) chưa được cấu hình. Vui lòng thiết lập API key trong user-secrets hoặc biến môi trường để kích hoạt tính năng phân tích khiếm khuyết tự động, hoặc tiến hành kiểm tra ngoại quan thủ công.",
                QualityStatus: "NeedsReview",
                ConfidenceScore: 0.0,
                SeverityLevel: "None",
                IsFallback: true,
                ErrorMessage: "Gemini API key is missing."
            );
        }

        var model = _configuration["Gemini:Model"] 
            ?? Environment.GetEnvironmentVariable("GEMINI_MODEL") 
            ?? "gemini-3.5-flash";
        var baseUrl = _configuration["Gemini:ApiUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        var requestUrl = $"{baseUrl}/models/{model}:generateContent?key={apiKey}";

        // Build prompt instructing structured JSON output with Vietnamese descriptions
        var systemPrompt = BuildInspectionPrompt(productName, category);
        var base64Image = Convert.ToBase64String(imageBytes);

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = systemPrompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType,
                                data = base64Image
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json",
                temperature = 0.2
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        try
        {
            _logger.LogInformation("Calling Gemini Vision API ({Model}) for product: {ProductName}", model, productName);
            var response = await _httpClient.PostAsync(requestUrl, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API returned error HTTP {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

                if ((int)response.StatusCode == 429)
                {
                    return new AiAnalysisResult(
                        CategoryName: "Không có lỗi (No Defect)",
                        DefectDescription: "Đã vượt quá giới hạn tần suất gọi API Gemini AI (HTTP 429). Vui lòng thử lại sau giây lát hoặc tiến hành kiểm định thủ công.",
                        QualityStatus: "NeedsReview",
                        ConfidenceScore: 0.0,
                        SeverityLevel: "None",
                        IsFallback: true,
                        ErrorMessage: "Rate limit exceeded (HTTP 429)."
                    );
                }

                return new AiAnalysisResult(
                    CategoryName: "Không có lỗi (No Defect)",
                    DefectDescription: $"Dịch vụ AI trả về mã lỗi HTTP {(int)response.StatusCode}. Báo cáo được chuyển sang danh sách chờ kiểm duyệt thủ công.",
                    QualityStatus: "NeedsReview",
                    ConfidenceScore: 0.0,
                    SeverityLevel: "None",
                    IsFallback: true,
                    ErrorMessage: $"HTTP {response.StatusCode}"
                );
            }

            var responseBodyString = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseGeminiResponse(responseBodyString);
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Gemini Vision request was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with Gemini Vision API.");
            return new AiAnalysisResult(
                CategoryName: "Không có lỗi (No Defect)",
                DefectDescription: "Không thể kết nối đến máy chủ AI Google Gemini (lỗi mạng hoặc hết thời gian chờ). Vui lòng kiểm tra kết nối mạng và thử lại.",
                QualityStatus: "NeedsReview",
                ConfidenceScore: 0.0,
                SeverityLevel: "None",
                IsFallback: true,
                ErrorMessage: ex.Message
            );
        }
    }

    private static string BuildInspectionPrompt(string? productName, string? category)
    {
        return $@"
Bạn là chuyên gia kiểm định chất lượng sản phẩm (QC Inspector) công nghiệp cao cấp.
Sản phẩm đang được kiểm định: {productName ?? "Linh kiện chưa xác định"} (Danh mục: {category ?? "Công nghiệp"}).

Nhiệm vụ:
Thực hiện phân tích thị giác nghiêm ngặt trên ảnh sản phẩm được cung cấp. Phát hiện các khuyết tật ngoại quan, lỗi gia công, lỗi bảo quản hoặc lỗi lắp ráp như: trầy xước bề mặt, nứt vỡ rạn nứt cấu trúc, móp méo cong vênh thân vỏ, biến màu hoen gỉ ăn mòn bề mặt, hoặc sai lệch hở khớp lắp ráp linh kiện.

Yêu cầu xuất ra (Output Requirement):
Bắt buộc trả về DUY NHẤT một đối tượng JSON hợp lệ theo cấu trúc sau:
{{
  ""categoryName"": ""Tên danh mục lỗi"",
  ""defectDescription"": ""Mô tả chi tiết bằng TIẾNG VIỆT về khiếm khuyết được phát hiện (vị trí, loại khuyết tật, mức độ ảnh hưởng) hoặc xác nhận bề mặt hoàn thiện đạt chuẩn xuất xưởng"",
  ""qualityStatus"": ""Passed"" | ""Failed"" | ""NeedsReview"",
  ""confidenceScore"": 0.95,
  ""severityLevel"": ""None"" | ""Low"" | ""Medium"" | ""High"" | ""Critical""
}}

Quy tắc phân loại (Classification Guidelines):
1. ""categoryName"" BẮT BUỘC phải là chính xác MỘT trong các giá trị sau:
   - ""Không có lỗi (No Defect)"" (bề mặt hoàn thiện hoàn hảo, đạt dung sai sản xuất)
   - ""Trầy xước bề mặt (Surface Scratch)"" (vết trầy xước, xước dăm, cọ xát)
   - ""Nứt gãy kết cấu (Structural Crack)"" (vết nứt, rạn nứt, đứt gãy kết cấu)
   - ""Móp méo / Biến dạng (Dent / Deformation)"" (vết lõm, móp méo, cong vênh)
   - ""Ăn mòn / Đổi màu (Discoloration / Corrosion)"" (ố màu, oxy hóa, gỉ sét, bong tróc sơn)
   - ""Lệch khớp lắp ráp (Assembly Misalignment)"" (khe hở, lệch ngàm, lỏng khớp, thiếu ốc vít)

2. ""qualityStatus"":
   - ""Passed"": Sản phẩm hoàn hảo, không có khuyết tật (categoryName phải là ""Không có lỗi (No Defect)"", severityLevel là ""None"").
   - ""Failed"": Phát hiện khuyết tật ngoại quan vượt ngưỡng cho phép.
   - ""NeedsReview"": Ảnh bị mờ, mất nét, tối, bị che khuất hoặc độ tin cậy AI không chắc chắn.

3. ""confidenceScore"": Số thực giữa 0.00 và 1.00.
4. ""defectDescription"": BẮT BUỘC VIẾT BẰNG TIẾNG VIỆT rõ ràng, khách quan, mang tính kỹ thuật công nghiệp chuẩn xác.
";
    }

    internal AiAnalysisResult ParseGeminiResponse(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                _logger.LogWarning("Gemini response contained no candidates.");
                return FallbackMalformed("Dịch vụ AI không trả về ứng viên kết quả nào.");
            }

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
            {
                return FallbackMalformed("Nội dung phản hồi từ AI bị rỗng.");
            }

            var textJson = parts[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(textJson))
            {
                return FallbackMalformed("Văn bản phản hồi từ AI không có dữ liệu.");
            }

            // Strip markdown code fences if present
            var cleanJson = textJson.Trim();
            if (cleanJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            using var parsedResult = JsonDocument.Parse(cleanJson);
            var pRoot = parsedResult.RootElement;

            var categoryName = pRoot.TryGetProperty("categoryName", out var catProp) ? catProp.GetString() ?? "Không có lỗi (No Defect)" : "Không có lỗi (No Defect)";
            var description = pRoot.TryGetProperty("defectDescription", out var descProp) ? descProp.GetString() ?? "Không có mô tả chi tiết." : "Không có mô tả chi tiết.";
            var status = pRoot.TryGetProperty("qualityStatus", out var statusProp) ? statusProp.GetString() ?? "NeedsReview" : "NeedsReview";
            var confidence = pRoot.TryGetProperty("confidenceScore", out var confProp) ? confProp.GetDouble() : 0.85;
            var severity = pRoot.TryGetProperty("severityLevel", out var sevProp) ? sevProp.GetString() ?? "None" : "None";

            categoryName = NormalizeCategoryName(categoryName);

            if (!status.Equals("Passed", StringComparison.OrdinalIgnoreCase) &&
                !status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                status = "NeedsReview";
            }
            else
            {
                status = char.ToUpperInvariant(status[0]) + status.Substring(1).ToLowerInvariant();
            }

            if (status.Equals("Passed", StringComparison.OrdinalIgnoreCase))
            {
                categoryName = "Không có lỗi (No Defect)";
                severity = "None";
            }

            return new AiAnalysisResult(
                CategoryName: categoryName,
                DefectDescription: description,
                QualityStatus: status,
                ConfidenceScore: Math.Clamp(confidence, 0.0, 1.0),
                SeverityLevel: severity,
                IsFallback: false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse structured JSON from Gemini response: {Response}", responseBody);
            return FallbackMalformed("Không thể đọc phản hồi cấu trúc JSON từ dịch vụ AI.");
        }
    }

    private static string NormalizeCategoryName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Không có lỗi (No Defect)";

        if (raw.Contains("Scratch", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Trầy", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Xước", StringComparison.OrdinalIgnoreCase))
            return "Trầy xước bề mặt (Surface Scratch)";

        if (raw.Contains("Crack", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Nứt", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Gãy", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("Rạn", StringComparison.OrdinalIgnoreCase))
            return "Nứt gãy kết cấu (Structural Crack)";

        if (raw.Contains("Dent", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Deformation", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Móp", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Méo", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("Biến dạng", StringComparison.OrdinalIgnoreCase))
            return "Móp méo / Biến dạng (Dent / Deformation)";

        if (raw.Contains("Corrosion", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Discoloration", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Gỉ", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Ăn mòn", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Đổi màu", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("Biến màu", StringComparison.OrdinalIgnoreCase))
            return "Ăn mòn / Đổi màu (Discoloration / Corrosion)";

        if (raw.Contains("Misalignment", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Assembly", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Lệch", StringComparison.OrdinalIgnoreCase) || 
            raw.Contains("Lắp ráp", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("Sai lệch", StringComparison.OrdinalIgnoreCase))
            return "Lệch khớp lắp ráp (Assembly Misalignment)";

        return "Không có lỗi (No Defect)";
    }

    private static AiAnalysisResult FallbackMalformed(string reason)
    {
        return new AiAnalysisResult(
            CategoryName: "Không có lỗi (No Defect)",
            DefectDescription: $"{reason} Ảnh kiểm định đã được lưu lại để nhân viên QC kiểm tra trực quan thủ công.",
            QualityStatus: "NeedsReview",
            ConfidenceScore: 0.0,
            SeverityLevel: "None",
            IsFallback: true,
            ErrorMessage: reason
        );
    }
}
