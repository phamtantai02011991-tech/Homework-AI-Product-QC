# System Workflow & Architecture Documentation

## AI Product Defect Inspection & Quality Control Assistant

---

## 1. High-Level System Architecture

Based on the official homework specification (Page 3), the system is divided into five core operational tiers:

```mermaid
flowchart LR
    subgraph S1["1. User Interface (React)"]
        UI1["Upload product image"]
        UI2["Select Product & Inspector"]
        UI3["View Live Preview & Result"]
        UI4["Dashboard QC Metrics"]
    end

    subgraph S2["2. Web API Backend (ASP.NET Core)"]
        API1["Receive multipart/form-data"]
        API2["Validate Magic Bytes & Size"]
        API3["Save to wwwroot/uploads"]
        API4["Base64 Image Encoding"]
    end

    subgraph S3["3. AI Vision Service (Gemini API)"]
        AI1["Multimodal Vision Analysis"]
        AI2["Enforce Structured JSON Output"]
        AI3["Vietnamese Defect Description"]
        AI4["Retry & Rate Limit Handling"]
    end

    subgraph S4["4. Review & Confirmation"]
        QC1["Display AI Extracted JSON"]
        QC2["QC Human Verification"]
        QC3["Override / Confirm 'Pass' or 'Fail'"]
        QC4["Explicit Submit"]
    end

    subgraph S5["5. Database Transaction (EF Core / SQL Server)"]
        DB1["Create IDbContextTransaction"]
        DB2["Verify Duplicate Confirmation"]
        DB3["Update Passed/Failed Stock"]
        DB4["Commit Transaction"]
    end

    S1 -->|POST /api/inspections/analyze| S2
    S2 -->|Base64 & Vietnamese Prompt| S3
    S3 -->|Structured JSON Result| S2
    S2 -->|Analysis DTO & ReportId| S1
    S1 -->|Display Analysis for Review| S4
    S4 -->|POST /api/inspections/{id}/confirm| S5
    S5 -->|Update Dashboard & Real-time Stats| S1
```

---

## 2. End-to-End Sequence Diagrams

### 2.1 Workflow 1: Image Upload & AI Analysis (`POST /api/inspections/analyze`)

> [!IMPORTANT]
> **Zero Inventory Mutation Rule:** During the analysis phase, the database strictly logs the inspection report in an unconfirmed state (`IsUserConfirmed = false`). **No product inventory counters are modified during this step.**

```mermaid
sequenceDiagram
    autonumber
    actor QC as QC Inspector (Client)
    participant UI as React Frontend
    participant API as ASP.NET Core API
    participant FS as Local File Storage (wwwroot/uploads)
    participant AI as Gemini Vision Service
    participant DB as SQL Server (EF Core)

    QC->>UI: Select Product, Inspector & Upload Image
    UI->>UI: Client-side preview & validate file type/size
    QC->>UI: Click "Start AI Defect Analysis" (Phân tích AI)
    UI->>API: POST /api/inspections/analyze (multipart/form-data: productId, inspectorId, file)
    
    API->>API: Validate file extension (.jpg, .png, .webp) & magic bytes
    API->>API: Generate secure GUID filename: qc_{guid}.jpg
    API->>FS: Save file stream to wwwroot/uploads/
    
    API->>API: Read file stream & encode to Base64
    API->>AI: Call Gemini 3.5 Flash API (Base64 payload + Vietnamese structured JSON schema prompt)
    
    alt Gemini API Success
        AI-->>API: 200 OK with Structured JSON (categoryName, defectDescription, qualityStatus, confidenceScore)
        API->>DB: Query DefectCategory by CategoryName (fallback to 'Không có lỗi' or 'Trầy xước')
    else Gemini API Error (Rate Limit 429 / Timeout / Network)
        AI-->>API: Error / Timeout
        API->>API: Fallback to manual inspection draft (QualityStatus: 'NeedsReview', Confidence: 0.0)
    end

    API->>DB: Insert InspectionReport (IsUserConfirmed: false, ImageUrl: /uploads/qc_{guid}.jpg)
    DB-->>API: Saved with ReportId
    API-->>UI: 200 OK (InspectionAnalysisResponseDto: reportId, imageUrl, defectCategory, description, qualityStatus, confidenceScore)
    UI->>UI: Transition to "Preview & Confirmation" screen
    UI-->>QC: Display visual comparison & AI diagnosis
```

---

### 2.2 Workflow 2: Human QC Review & Confirmation (`POST /api/inspections/{id}/confirm`)

> [!IMPORTANT]
> **Atomic Inventory Update Rule:** The database update uses an explicit `IDbContextTransaction`. If stock updates or report updates fail, the entire transaction is rolled back.

```mermaid
sequenceDiagram
    autonumber
    actor QC as QC Inspector (Client)
    participant UI as React Frontend
    participant API as ASP.NET Core API
    participant DB as SQL Server (ApplicationDbContext)

    QC->>UI: Review AI classification & Vietnamese description
    Note over QC, UI: QC may accept AI verdict or override Status / Defect Category
    QC->>UI: Click "Confirm & Update Inventory" (Xác nhận tồn kho)
    UI->>API: POST /api/inspections/{id}/confirm (Body: { qualityStatus, defectCategoryId, notes })

    API->>DB: Begin IDbContextTransactionAsync()
    API->>DB: SELECT InspectionReport WHERE ReportId = {id} WITH Concurrency Token
    
    alt Report Not Found
        API-->>UI: 404 Not Found (Report does not exist)
    else Report Already Confirmed (IsUserConfirmed == true)
        API->>DB: Rollback Transaction
        API-->>UI: 409 Conflict ("Báo cáo kiểm tra đã được xác nhận trước đó!")
    else Report Valid for Confirmation
        API->>DB: SELECT Product WHERE ProductId = report.ProductId
        
        alt QualityStatus == "Passed"
            API->>DB: Product.PassedStock = Product.PassedStock + 1
        else QualityStatus == "Failed"
            API->>DB: Product.FailedStock = Product.FailedStock + 1
        end
        
        API->>DB: Ensure Product.TotalStock >= (PassedStock + FailedStock)
        API->>DB: Update InspectionReport: IsUserConfirmed = true, QualityStatus, DefectCategoryId
        API->>DB: SaveChangesAsync()
        API->>DB: CommitTransactionAsync()
        
        API-->>UI: 200 OK (InspectionConfirmationResultDto: updated report & stock levels)
        UI->>UI: Show Toast "Xác nhận thành công, tồn kho đã được cập nhật!"
        UI->>API: GET /api/dashboard/summary
        API-->>UI: Updated dashboard metrics
        UI-->>QC: Refresh Dashboard & History views
    end
```

---

## 3. Inspection Report Lifecycle & State Machine

```mermaid
stateDiagram-v2
    [*] --> Uploading : User selects image in UI
    Uploading --> ImageStored : File uploaded to wwwroot/uploads
    ImageStored --> AIEvaluating : Base64 sent to Gemini API
    
    AIEvaluating --> UnconfirmedDraft : AI returns classification & Vietnamese JSON
    AIEvaluating --> FallbackDraft : AI Timeout / Rate Limit / Error
    
    state UnconfirmedDraft {
        [*] --> SuggestedPassed : Defect not detected (Conf >= 0.8)
        [*] --> SuggestedFailed : Defect detected (Conf >= 0.7)
        [*] --> NeedsReview : Low confidence (< 0.7) / Blurry image
    }

    state FallbackDraft {
        [*] --> ManualReviewRequired : Service unavailable / Network failure
    }

    UnconfirmedDraft --> ConfirmedPassed : Human confirms as 'Passed' (TotalStock / PassedStock +1)
    UnconfirmedDraft --> ConfirmedFailed : Human confirms as 'Failed' (TotalStock / FailedStock +1)
    FallbackDraft --> ConfirmedPassed : Human inspects manually & confirms Passed
    FallbackDraft --> ConfirmedFailed : Human inspects manually & confirms Failed
    
    ConfirmedPassed --> [*] : Locked / Immutable Audit Record
    ConfirmedFailed --> [*] : Locked / Immutable Audit Record
```

---

## 4. Edge Cases & Resilience Strategy

| Scenario | Risk | Mitigation Architecture |
| :--- | :--- | :--- |
| **Blurry / Unclear Image** | AI misclassifies defect or outputs false positive | System prompt directs Gemini to detect image clarity. If blurry or unreadable, Gemini outputs `QualityStatus: "NeedsReview"` and `ConfidenceScore < 0.5`. UI flags the item with an amber warning badge, disabling automatic approval. |
| **Gemini Rate Limit (HTTP 429) / Offline** | Inspection process fails abruptly | Backend catches the error and creates an unconfirmed draft report in `NeedsReview` state with an explanatory message (`isAiFallback: true`), returning HTTP 200 OK to allow manual inspection and avoid a 500/503 crash. |
| **Duplicate Confirmation** | User double-clicks "Confirm", inflating inventory twice | Front-end disables the button immediately upon click. Backend checks `IsUserConfirmed` inside the `IDbContextTransaction`. If `true`, throws HTTP 409 Conflict. EF Core `RowVersion` handles concurrent race conditions. |
| **Database Failure during Update** | Stock updated but report not confirmed (data desync) | All updates are wrapped inside `using var transaction = await _context.Database.BeginTransactionAsync()`. Any failure executes `transaction.RollbackAsync()`. |
| **Unsafe File Upload** | Malicious script uploaded (`.php`, `.exe`, path traversal) | File extension whitelist (`.jpg, .jpeg, .png, .webp`). Magic byte header verification. File renamed to random GUID. Stored outside source directories in `wwwroot/uploads`. |
| **Non-null DefectCategoryId on Passed Items** | Foreign key violation when no defect exists | Database seeds `DefectCategoryId = 1` with name `"Không có lỗi (No Defect)"` and severity `"None"`. All Passed inspections are mapped to Category ID 1. |
