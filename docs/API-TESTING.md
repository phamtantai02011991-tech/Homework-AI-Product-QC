# API Testing Guide & Scenario Validation

## AI Product Defect Inspection & Quality Control Assistant

---

## 1. Quick Testing Overview

This guide provides PowerShell and cURL scripts to test all RESTful endpoints and execute the 15 mandatory test scenarios specified in the project requirements.

**Base URL:** `http://localhost:5000/api` (or `https://localhost:7001/api`)

---

## 2. Endpoint Verification Scripts (cURL & PowerShell)

### 2.1 Get All Products
```bash
# cURL
curl -X GET "http://localhost:5000/api/products" -H "Accept: application/json"

# PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Get | ConvertTo-Json -Depth 3
```

### 2.2 Get Inspectors & Categories
```bash
# Inspectors
curl -X GET "http://localhost:5000/api/inspectors"

# Defect Categories
curl -X GET "http://localhost:5000/api/defect-categories"
```

### 2.3 Analyze Image (`multipart/form-data`)
```bash
# cURL
curl -X POST "http://localhost:5000/api/inspections/analyze" \
  -F "productId=1" \
  -F "inspectorId=1" \
  -F "file=@./sample_images/02_minor_defect_scratch.jpg;type=image/jpeg"

# PowerShell
$form = @{
    productId = "1"
    inspectorId = "1"
    file = Get-Item -Path "./sample_images/02_minor_defect_scratch.jpg"
}
Invoke-RestMethod -Uri "http://localhost:5000/api/inspections/analyze" -Method Post -Form $form
```

### 2.4 Confirm Inspection Report
```bash
# cURL
curl -X POST "http://localhost:5000/api/inspections/1/confirm" \
  -H "Content-Type: application/json" \
  -d '{"qualityStatus": "Failed", "defectCategoryId": 2, "defectDescription": "Xác nhận có vết trầy bề mặt."}'

# PowerShell
$body = @{
    qualityStatus = "Failed"
    defectCategoryId = 2
    defectDescription = "Xác nhận có vết trầy bề mặt."
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/inspections/1/confirm" `
  -Method Post -ContentType "application/json" -Body $body
```

### 2.5 Get Dashboard Summary
```bash
# cURL
curl -X GET "http://localhost:5000/api/dashboard/summary"

# PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/summary" -Method Get | ConvertTo-Json -Depth 4
```

---

## 3. Mandatory 15 Test Scenarios Matrix

| Scenario # | Test Description | Input Condition | Expected Result & HTTP Status | Validation Point |
| :---: | :--- | :--- | :--- | :--- |
| **01** | **Clear image of a good product** | High-res image of pristine product | **200 OK**: `qualityStatus: "Passed"`, `confidenceScore >= 0.85`, `defectCategory: "Không có lỗi (No Defect)"`. Inventory unchanged. | Visual AI accuracy |
| **02** | **Image containing minor defect** | Image showing surface scratch or slight discoloration | **200 OK**: `qualityStatus: "Failed"`, `severityLevel: "Low"`, description notes scratch in Vietnamese. Inventory unchanged. | Defect sensitivity |
| **03** | **Image containing major defect** | Image showing structural crack or severe deformation | **200 OK**: `qualityStatus: "Failed"`, `severityLevel: "High"`, `defectCategory: "Nứt gãy kết cấu (Structural Crack)"`, description explains structural damage in Vietnamese. | Severity categorization |
| **04** | **Blurry or unclear image** | Deliberately blurred or dark image | **200 OK**: `qualityStatus: "NeedsReview"`, `confidenceScore < 0.60`, warning message advising manual inspection. | Low confidence handling |
| **05** | **Invalid file type** | Uploading `.exe`, `.pdf`, `.txt` file | **400 Bad Request**: Detail indicates unsupported file type. Magic byte validation rejection. | Upload security |
| **06** | **Exceeding upload limit** | File size > 10MB | **400 Bad Request**: Payload size exceeds max allowed size. | Denial of Service protection |
| **07** | **Missing or invalid ProductId** | `productId = 9999` (non-existent) | **404 Not Found** or **400 Bad Request**: "Sản phẩm không tồn tại trong hệ thống." | Foreign key validation |
| **08** | **Invalid InspectorId or CategoryId** | Non-existent inspector in analyze request | **400 Bad Request**: "Mã kiểm định viên không hợp lệ." | Reference integrity |
| **09** | **Gemini API failure / offline** | Mock network timeout or missing/invalid API key | **200 OK (Graceful Degradation)**: Clean fallback report created in `NeedsReview` state with `isAiFallback: true`. No system crash. Inventory unchanged. | Graceful degradation |
| **10** | **Malformed AI JSON** | Gemini returns non-JSON or missing fields | Fallback parser sets `NeedsReview` with raw response summary. System does not throw unhandled exception. | Robust JSON parsing |
| **11** | **Confirmation of Passed report** | Confirm report with `qualityStatus: "Passed"` | **200 OK**: `Product.PassedStock` increments by 1. `IsUserConfirmed` becomes `true`. | Atomic inventory update |
| **12** | **Confirmation of Failed report** | Confirm report with `qualityStatus: "Failed"` | **200 OK**: `Product.FailedStock` increments by 1. `IsUserConfirmed` becomes `true`. | Defect count accounting |
| **13** | **Repeated confirmation** | Sending confirm request to already confirmed report | **409 Conflict**: "Báo cáo kiểm tra đã được xác nhận trước đó." Stock does not double count. | Idempotency guard |
| **14** | **DB failure during confirmation** | Simulate DB fault before commit | **500 Internal Error**: `IDbContextTransaction.Rollback()` triggers. Neither stock nor report state is partially saved. | Transaction atomicity |
| **15** | **Concurrent confirmation attempts** | Two simultaneous requests confirming report #1 | One request succeeds (200 OK), the other receives **409 Conflict** due to concurrency token / state lock. | Race condition safety |
