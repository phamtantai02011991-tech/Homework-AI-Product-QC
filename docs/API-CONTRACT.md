# RESTful API Contract & Specification

## AI Product Defect Inspection & Quality Control Assistant

Base URL: `http://localhost:5000/api` or `https://localhost:7001/api`  
Swagger UI: `http://localhost:5000/swagger`

---

## Standard Error Response Format (RFC 7807 ProblemDetails)

All error responses from the API adhere to the standardized format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "File không hợp lệ hoặc kích thước vượt quá giới hạn cho phép (10MB).",
  "errors": {
    "file": ["Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp"]
  },
  "traceId": "00-843849384938-483948394-00"
}
```

---

## 1. Product Endpoints

### 1.1 List Products
`GET /api/products`

Retrieves a list of all products in the catalog with their current stock levels.

- **Request Headers:** None
- **Query Parameters:**
  - `search` (string, optional): Search by product name or category.
- **Response 200 OK:**
```json
[
  {
    "productId": 1,
    "productName": "Màn hình OLED 6.7 inch FHD+",
    "category": "Linh kiện điện tử",
    "totalStock": 500,
    "passedStock": 450,
    "failedStock": 12,
    "uninspectedStock": 38,
    "createdAt": "2026-09-24T08:00:00Z"
  },
  {
    "productId": 2,
    "productName": "Vỏ nhôm Anode động cơ servo",
    "category": "Cơ khí chính xác",
    "totalStock": 250,
    "passedStock": 210,
    "failedStock": 18,
    "uninspectedStock": 22,
    "createdAt": "2026-10-04T08:00:00Z"
  }
]
```

---

### 1.2 Get Product Details
`GET /api/products/{id}`

Retrieves details of a specific product along with its recent inspection summary.

- **Route Parameters:** `id` (int, required)
- **Response 200 OK:**
```json
{
  "productId": 1,
  "productName": "Màn hình OLED 6.7 inch FHD+",
  "category": "Linh kiện điện tử",
  "totalStock": 500,
  "passedStock": 450,
  "failedStock": 12,
  "uninspectedStock": 38,
  "passRatePercentage": 97.4,
  "createdAt": "2026-09-24T08:00:00Z",
  "recentInspectionsCount": 15
}
```
- **Response 404 Not Found:**
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Không tìm thấy sản phẩm có mã ID = 99."
}
```

---

### 1.3 Create Product
`POST /api/products`

Registers a new product batch into the system.

- **Request Body (JSON):**
```json
{
  "productName": "Cụm tản nhiệt hợp kim đồng",
  "category": "Cơ khí điện tử",
  "totalStock": 100
}
```
- **Validation Rules:**
  - `productName`: Required, max 200 chars.
  - `category`: Required, max 100 chars.
  - `totalStock`: Optional, integer >= 0. Default: 0.
- **Response 201 Created:**
```json
{
  "productId": 6,
  "productName": "Cụm tản nhiệt hợp kim đồng",
  "category": "Cơ khí điện tử",
  "totalStock": 100,
  "passedStock": 0,
  "failedStock": 0,
  "uninspectedStock": 100,
  "createdAt": "2026-10-24T01:50:00Z"
}
```

---

## 2. Inspector & Defect Category Endpoints

### 2.1 List Inspectors
`GET /api/inspectors`

Retrieves active QC inspectors for dropdown selection in the UI.

- **Response 200 OK:**
```json
[
  {
    "inspectorId": 1,
    "inspectorName": "Nguyễn Văn An",
    "department": "QC Điện Tử & Màn Hình",
    "email": "an.nguyen@factoryqc.vn"
  },
  {
    "inspectorId": 2,
    "inspectorName": "Trần Thị Bích",
    "department": "QC Gia Công Cơ Khí",
    "email": "bich.tran@factoryqc.vn"
  }
]
```

---

### 2.2 List Defect Categories
`GET /api/defect-categories`

Retrieves all defect classification categories.

- **Response 200 OK:**
```json
[
  {
    "defectCategoryId": 1,
    "categoryName": "Không có lỗi (No Defect)",
    "severityLevel": "None",
    "description": "Sản phẩm đạt chuẩn, bề mặt hoàn thiện tốt, không phát hiện khuyết tật ngoại quan."
  },
  {
    "defectCategoryId": 2,
    "categoryName": "Trầy xước bề mặt (Surface Scratch)",
    "severityLevel": "Low",
    "description": "Vết xước mảnh hoặc cọ xát trên bề mặt vỏ hoặc linh kiện, không ảnh hưởng kết cấu."
  },
  {
    "defectCategoryId": 3,
    "categoryName": "Nứt gãy kết cấu (Structural Crack)",
    "severityLevel": "High",
    "description": "Đường nứt hoặc rạn vỡ thân sản phẩm làm giảm độ bền cơ học và an toàn."
  }
]
```

---

## 3. Inspection & AI Analysis Endpoints

### 3.1 AI Analyze Inspection (`multipart/form-data`)
`POST /api/inspections/analyze`

Uploads an image, verifies it, stores it in `wwwroot/uploads`, triggers Gemini Vision analysis with structured Vietnamese JSON response, and persists an unconfirmed inspection draft.

> [!WARNING]
> This endpoint **NEVER** updates product stock.

- **Request Content-Type:** `multipart/form-data`
- **Form Data Parameters:**
  - `productId` (int, required): Target product ID.
  - `inspectorId` (int, required): Operating inspector ID.
  - `file` (binary, required): Image file (`.jpg`, `.jpeg`, `.png`, `.webp`, max 10MB).
- **Processing Logic:**
  1. Validates `productId` and `inspectorId` existence in database.
  2. Validates image extension, file signature (magic bytes), and file size.
  3. Saves file to `wwwroot/uploads/qc_{guid}.jpg`.
  4. Encodes image to Base64 and invokes Google Gemini API with system instructions:
     - Output JSON Schema: `{ categoryName: string, defectDescription: string, qualityStatus: "Passed" | "Failed" | "NeedsReview", confidenceScore: number }`.
     - Generate Vietnamese descriptions.
  5. Maps `categoryName` to `DefectCategory` in database (defaults to ID 1 if Passed).
  6. Creates `InspectionReport` entity with `IsUserConfirmed: false`.
- **Response 200 OK (AI Success):**
```json
{
  "reportId": 101,
  "productId": 1,
  "productName": "Màn hình OLED 6.7 inch FHD+",
  "inspectorId": 1,
  "inspectorName": "Nguyễn Văn An",
  "defectCategoryId": 2,
  "defectCategoryName": "Trầy xước bề mặt (Surface Scratch)",
  "severityLevel": "Low",
  "imageUrl": "/uploads/qc_e4d2938a-1294-482a-a92c.jpg",
  "defectDescription": "Phát hiện vết trầy xước mảnh dài khoảng 1.5cm ở góc trên bên phải bề mặt màn hình.",
  "qualityStatus": "Failed",
  "confidenceScore": 0.94,
  "isUserConfirmed": false,
  "createdAt": "2026-10-24T01:52:00Z",
  "isAiFallback": false,
  "warningMessage": null
}
```

- **Response 200 OK (AI Blurry/Low Confidence / Needs Review):**
```json
{
  "reportId": 102,
  "productId": 1,
  "productName": "Màn hình OLED 6.7 inch FHD+",
  "inspectorId": 1,
  "inspectorName": "Nguyễn Văn An",
  "defectCategoryId": 1,
  "defectCategoryName": "Không có lỗi (No Defect)",
  "severityLevel": "None",
  "imageUrl": "/uploads/qc_b9281a9c-9821-4821-bbcd.jpg",
  "defectDescription": "Hình ảnh bị mờ hoặc góc chụp bị chói sáng, không thể đánh giá chính xác bề mặt. Yêu cầu kiểm tra thủ công.",
  "qualityStatus": "NeedsReview",
  "confidenceScore": 0.45,
  "isUserConfirmed": false,
  "createdAt": "2026-10-24T01:53:00Z",
  "isAiFallback": false,
  "warningMessage": null
}
```

- **Response 200 OK (Graceful Degradation — Gemini API Down / Missing Key / Offline Fallback):**
> **Resilience Guarantee:** When the external Gemini API is unreachable, rate limited (HTTP 429), or missing credentials, the backend **does not crash** or return HTTP 503. Instead, it saves an unconfirmed draft inspection report with `qualityStatus: "NeedsReview"`, `confidenceScore: 0.0`, `isAiFallback: true`, and returns HTTP 200 OK so the QC operator can proceed with manual inspection seamlessly.

```json
{
  "reportId": 103,
  "productId": 1,
  "productName": "Màn hình OLED 6.7 inch FHD+",
  "inspectorId": 1,
  "inspectorName": "Nguyễn Văn An",
  "defectCategoryId": 1,
  "defectCategoryName": "Không có lỗi (No Defect)",
  "severityLevel": "None",
  "imageUrl": "/uploads/qc_c1234567-89ab-cdef-0123.jpg",
  "defectDescription": "Khóa API Google Gemini (GEMINI_API_KEY) chưa được cấu hình. Vui lòng thiết lập API key trong user-secrets hoặc biến môi trường để kích hoạt tính năng phân tích khiếm khuyết tự động, hoặc tiến hành kiểm tra ngoại quan thủ công.",
  "qualityStatus": "NeedsReview",
  "confidenceScore": 0.0,
  "isUserConfirmed": false,
  "createdAt": "2026-10-24T01:54:00Z",
  "isAiFallback": true,
  "warningMessage": "Gemini API key is missing."
}
```

- **Response 400 Bad Request:**
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Tệp tải lên không phải là ảnh hợp lệ hoặc định dạng không được hỗ trợ."
}
```

- **Response 404 Not Found:**
```json
{
  "status": 404,
  "title": "Product Not Found",
  "detail": "Product with ID = 999 does not exist in the system."
}
```

---

### 3.2 Confirm Inspection Report & Update Inventory
`POST /api/inspections/{id}/confirm`

Explicitly confirms a quality control inspection report and atomically updates product inventory (`PassedStock` or `FailedStock`) within an `IDbContextTransaction`.

- **Route Parameters:** `id` (int, required) — The `ReportId`
- **Request Body (JSON, Optional Adjustments):**
```json
{
  "qualityStatus": "Failed",
  "defectCategoryId": 2,
  "defectDescription": "Xác nhận lỗi: Vết trầy xước bề mặt kính màn hình góc trên phải (QC ghi nhận thêm)."
}
```
*Note: If request body fields are omitted, the report's current AI parameters will be used.*

- **Processing Logic:**
  1. Begins EF Core transaction `_context.Database.BeginTransactionAsync()`.
  2. Finds `InspectionReport` by `id`.
  3. **Duplicate Check:** If `IsUserConfirmed == true`, aborts and returns HTTP 409 Conflict.
  4. **Status Validation:** `qualityStatus` must be either `"Passed"` or `"Failed"`. A status of `"NeedsReview"` cannot be confirmed without resolution.
  5. Finds associated `Product`:
     - If `qualityStatus == "Passed"`: `Product.PassedStock += 1`.
     - If `qualityStatus == "Failed"`: `Product.FailedStock += 1`.
     - Updates `Product.TotalStock = Math.Max(Product.TotalStock, Product.PassedStock + Product.FailedStock)`.
  6. Updates `InspectionReport`:
     - `IsUserConfirmed = true`.
     - `QualityStatus = confirmedStatus`.
     - `DefectCategoryId = confirmedCategoryId`.
     - `DefectDescription = confirmedDescription`.
  7. Commits transaction and releases locks.
- **Response 200 OK:**
```json
{
  "reportId": 101,
  "productId": 1,
  "productName": "Màn hình OLED 6.7 inch FHD+",
  "qualityStatus": "Failed",
  "defectCategoryName": "Trầy xước bề mặt (Surface Scratch)",
  "isUserConfirmed": true,
  "confirmedAt": "2026-10-24T01:55:00Z",
  "updatedInventory": {
    "totalStock": 500,
    "passedStock": 450,
    "failedStock": 13,
    "uninspectedStock": 37
  },
  "message": "Xác nhận báo cáo thành công. Số lượng hàng lỗi (FailedStock) đã được tăng thêm 1."
}
```

- **Response 409 Conflict (Duplicate Confirmation Prevention):**
```json
{
  "status": 409,
  "title": "Conflict",
  "detail": "Báo cáo kiểm tra mã #101 đã được xác nhận trước đó. Không thể xác nhận lại để tránh sai lệch tồn kho!"
}
```

- **Response 422 Unprocessable Entity:**
```json
{
  "status": 422,
  "title": "Unprocessable Entity",
  "detail": "Trạng thái 'NeedsReview' không thể xác nhận. Vui lòng chọn kết luận 'Passed' hoặc 'Failed'."
}
```

---

### 3.3 List Inspection Reports (History)
`GET /api/inspections`

Retrieves a paginated, filterable list of inspection reports.

- **Query Parameters:**
  - `page` (int, default: 1)
  - `pageSize` (int, default: 10, max: 100)
  - `productId` (int, optional)
  - `status` (string, optional: `Passed`, `Failed`, `NeedsReview`)
  - `isConfirmed` (bool, optional: `true`, `false`)
  - `startDate` (datetime, optional)
  - `endDate` (datetime, optional)
- **Response 200 OK:**
```json
{
  "totalItems": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5,
  "items": [
    {
      "reportId": 101,
      "productId": 1,
      "productName": "Màn hình OLED 6.7 inch FHD+",
      "inspectorId": 1,
      "inspectorName": "Nguyễn Văn An",
      "defectCategoryId": 2,
      "defectCategoryName": "Trầy xước bề mặt (Surface Scratch)",
      "severityLevel": "Low",
      "imageUrl": "/uploads/qc_e4d2938a-1294-482a-a92c.jpg",
      "qualityStatus": "Failed",
      "confidenceScore": 0.94,
      "isUserConfirmed": true,
      "createdAt": "2026-10-24T01:52:00Z"
    }
  ]
}
```

---

### 3.4 Get Inspection Report Details
`GET /api/inspections/{id}`

Retrieves full details for a single inspection report.

- **Route Parameters:** `id` (int, required)
- **Response 200 OK:**
```json
{
  "reportId": 101,
  "product": {
    "productId": 1,
    "productName": "Màn hình OLED 6.7 inch FHD+",
    "category": "Linh kiện điện tử"
  },
  "inspector": {
    "inspectorId": 1,
    "inspectorName": "Nguyễn Văn An",
    "department": "QC Điện Tử & Màn Hình",
    "email": "an.nguyen@factoryqc.vn"
  },
  "defectCategory": {
    "defectCategoryId": 2,
    "categoryName": "Trầy xước bề mặt (Surface Scratch)",
    "severityLevel": "Low",
    "description": "Vết xước mảnh hoặc cọ xát trên bề mặt vỏ hoặc linh kiện, không ảnh hưởng kết cấu."
  },
  "imageUrl": "/uploads/qc_e4d2938a-1294-482a-a92c.jpg",
  "defectDescription": "Phát hiện vết trầy xước mảnh dài khoảng 1.5cm ở góc trên bên phải bề mặt màn hình.",
  "qualityStatus": "Failed",
  "confidenceScore": 0.94,
  "isUserConfirmed": true,
  "createdAt": "2026-10-24T01:52:00Z"
}
```

---

## 4. Dashboard Endpoints

### 4.1 QC Dashboard Summary
`GET /api/dashboard/summary`

Aggregates high-level QC analytics for executive and warehouse monitoring.

- **Response 200 OK:**
```json
{
  "totalProducts": 5,
  "totalInspections": 128,
  "totalConfirmed": 120,
  "totalPendingConfirmation": 8,
  "totalPassed": 110,
  "totalFailed": 10,
  "passRate": 91.67,
  "defectDistribution": [
    { "categoryName": "Trầy xước bề mặt (Surface Scratch)", "count": 5, "percentage": 50.0 },
    { "categoryName": "Nứt gãy kết cấu (Structural Crack)", "count": 3, "percentage": 30.0 },
    { "categoryName": "Móp méo / Biến dạng (Dent / Deformation)", "count": 2, "percentage": 20.0 }
  ],
  "recentInspections": [
    {
      "reportId": 101,
      "productName": "Màn hình OLED 6.7 inch FHD+",
      "inspectorName": "Nguyễn Văn An",
      "qualityStatus": "Failed",
      "confidenceScore": 0.94,
      "isUserConfirmed": true,
      "createdAt": "2026-10-24T01:52:00Z"
    }
  ]
}
```
