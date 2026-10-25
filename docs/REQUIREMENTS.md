# Requirements Analysis & Traceability Matrix

## AI Product Defect Inspection & Quality Control Assistant

---

## 1. Project Overview & Scope

The **AI Product Defect Inspection & Quality Control Assistant** is an educational, production-ready full-stack web application designed for manufacturing and warehouse quality control (QC). The system leverages Multimodal Generative AI (Google Gemini Vision) to inspect product images, detect surface and structural defects, classify defect categories, generate localized Vietnamese descriptions, log inspection reports, and atomically update product stock counters upon explicit human QC confirmation.

### Core Objectives
1. **Multimodal AI Defect Detection:** Analyze visual characteristics of manufactured products from uploaded images.
2. **Vietnamese Language Optimization:** Produce defect classifications and descriptions tailored for Vietnamese factory QC personnel.
3. **Human-in-the-Loop Integrity:** Strictly separate AI suggestion from inventory updates. Inventory status is updated **only** after explicit user confirmation.
4. **Data Integrity & Atomic Transactions:** Ensure database consistency using EF Core database transactions (`IDbContextTransaction`) during inventory updates, with concurrency and duplicate-confirmation protection.
5. **Educational Excellence:** Clear layered architecture, robust error handling, standard coding conventions, thorough documentation, and seamless presentation readiness.

---

## 2. Requirements Breakdown & Traceability Matrix

| Req ID | Source Reference | Category | Requirement Description | Verification Method |
| :--- | :--- | :--- | :--- | :--- |
| **REQ-R1.1** | Homework.pdf R1 / Instr. §3 | Database | Create `Product` entity with fields: `ProductId` (PK), `ProductName` (nvarchar 200), `Category` (nvarchar 100), `TotalStock` (int), `PassedStock` (int), `FailedStock` (int), `CreatedAt` (datetime). | Code Inspection & EF Migration script |
| **REQ-R1.2** | Homework.pdf R1 / Instr. §3 | Database | Create `Inspector` entity with fields: `InspectorId` (PK), `InspectorName` (nvarchar 100), `Department` (nvarchar 100), `Email` (nvarchar 100). | Code Inspection & EF Migration script |
| **REQ-R1.3** | Homework.pdf R1 / Instr. §3 | Database | Create `DefectCategory` entity with fields: `DefectCategoryId` (PK), `CategoryName` (nvarchar 100), `SeverityLevel` (nvarchar 50), `Description` (nvarchar 500). | Code Inspection & EF Migration script |
| **REQ-R1.4** | Homework.pdf R1 / Instr. §3 | Database | Create `InspectionReport` entity with fields: `ReportId` (PK), `ProductId` (FK), `InspectorId` (FK), `DefectCategoryId` (FK), `ImageUrl` (nvarchar 500), `DefectDescription` (nvarchar max), `QualityStatus` (nvarchar 50), `ConfidenceScore` (float), `IsUserConfirmed` (bit), `CreatedAt` (datetime). | Code Inspection & EF Migration script |
| **REQ-R1.5** | Homework.pdf R1 / Instr. §3 | Database | Configure 1:N relationships (`Product` 1:N `InspectionReport`, `Inspector` 1:N `InspectionReport`, `DefectCategory` 1:N `InspectionReport`) using EF Core Fluent API with `DeleteBehavior.Restrict`. | EF Core DbContext inspection & DB schema check |
| **REQ-R1.6** | Homework.pdf R1 / Instr. §3 | Database | Seed initial realistic data for Products, Inspectors, and DefectCategories (must include a "No Defect" / "Không phát hiện lỗi" category for non-defective items). | Database Seed Execution & Verification |
| **REQ-R2.1** | Homework.pdf R2 / Instr. §6 | Frontend | **QC Dashboard Page:** Displays summary metrics (Total Products, Total Inspected Reports, Passed Stock, Failed Stock, Pass Rate, Defect Breakdown) and recent inspections table with loading/empty/error states. | UI Testing & API Integration |
| **REQ-R2.2** | Homework.pdf R2 / Instr. §6 | Frontend | **Upload Inspection Form Page:** Product selector, Inspector selector, drag-and-drop image upload, live image preview, validation feedback, and progress indicator. | UI Testing & File Validation |
| **REQ-R2.3** | Homework.pdf R2 / Instr. §6 | Frontend | **Preview & Confirmation Page:** Displays uploaded image side-by-side with AI analysis (Defect Category, Vietnamese Description, Quality Status, Confidence Score), manual review warnings, Confirm action, and Cancel/Retry action. | UI Testing & Workflow Verification |
| **REQ-R2.4** | Homework.pdf R2 / Instr. §6 | Frontend | **Inspection History Page:** Tabular view of all reports with filters by Status, Product, Date range, search bar, pagination, and navigation to Detail. | UI Testing & Filter Verification |
| **REQ-R2.5** | Homework.pdf R2 / Instr. §6 | Frontend | **Inspection Detail Page:** Dedicated view of a single inspection report showing high-resolution image, AI diagnostic parameters, inspector data, and confirmation status. | UI Navigation & Detail Inspection |
| **REQ-R3.1** | Homework.pdf R3 / Instr. §4, §5 | Backend | Build clean 3-tier architecture: `AIProductQC.API`, `AIProductQC.Core`, `AIProductQC.Infrastructure`. | Solution Structure & Dependency Check |
| **REQ-R3.2** | Homework.pdf R3 / Instr. §5 | Backend | Implement static file management for image uploads stored securely in `wwwroot/uploads` with unique GUID filenames and sanitized paths. | File System & Static URL Check |
| **REQ-R3.3** | Homework.pdf R3 / Instr. §5 | Backend | RESTful endpoints: `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `GET /api/inspectors`, `GET /api/defect-categories`, `GET /api/inspections`, `GET /api/inspections/{id}`, `POST /api/inspections/analyze`, `POST /api/inspections/{id}/confirm`, `GET /api/dashboard/summary`. | Swagger & Automated Endpoint Tests |
| **REQ-R4.1** | Homework.pdf R4 / Instr. §5 | AI Integration | Multimodal image analysis using Google Gemini API. Read API key securely from backend configuration (`appsettings.json` / user secrets / env var `GEMINI_API_KEY`). Never expose to frontend. | Configuration & Network Tracing |
| **REQ-R4.2** | Homework.pdf R4 / Instr. §5 | AI Integration | Image encoding to Base64 with proper MIME type mapping and structured JSON output schema enforcement (`response_mime_type: "application/json"`). | Gemini Vision Service unit & integration test |
| **REQ-R4.3** | Homework.pdf R4 / Instr. §5 | AI Integration | AI prompt engineering optimized for Vietnamese defect descriptions and defect category mapping. | Inspection Report Content Verification |
| **REQ-R4.4** | Homework.pdf R4 / Instr. §5 | AI Integration | Comprehensive error handling for blurry, unclear, or invalid images, API timeouts, rate limits (HTTP 429), and malformed AI JSON. Never fabricate fake results. | Fault Injection & Negative Testing |
| **REQ-R5.1** | Homework.pdf R5 / Instr. §5 | Inventory & QC | Strictly prevent automatic inventory updates during the `POST /api/inspections/analyze` phase. | Database State Verification after Analyze |
| **REQ-R5.2** | Homework.pdf R5 / Instr. §5 | Inventory & QC | Explicit confirmation via `POST /api/inspections/{id}/confirm` executes an atomic EF Core transaction (`IDbContextTransaction`) updating `PassedStock` or `FailedStock` on `Product` and setting `IsUserConfirmed = true`. | Database Transaction Testing |
| **REQ-R5.3** | Homework.pdf R5 / Instr. §5 | Inventory & QC | Idempotency & duplicate confirmation prevention: Return HTTP 400/409 if a report is already confirmed. Concurrency protection via EF Core RowVersion / ConcurrencyCheck. | Concurrency & Duplicate Request Testing |
| **REQ-R6.1** | Homework.pdf R6 / Instr. §8 | Testing | Validate 15 mandatory test scenarios: (1) Clear good product, (2) Minor defects, (3) Major defects, (4) Blurry/unclear image, (5) Invalid file type, (6) Exceeding file size, (7) Missing/invalid ProductId, (8) Invalid InspectorId/DefectCategoryId, (9) Gemini API failure, (10) Malformed AI JSON, (11) Confirmation of Passed report, (12) Confirmation of Failed report, (13) Repeated confirmation, (14) DB failure during confirmation, (15) Concurrent confirmation attempts. | Test Execution Matrix & Log Verification |
| **REQ-R6.2** | Homework.pdf R6 / Instr. §10 | Documentation | Complete documentation (`README.md`, `ERD.md`, `WORKFLOW.md`, `API-CONTRACT.md`, `IMPLEMENTATION-PLAN.md`, API testing guide with curl/Postman scripts). | Document Review & Delivery Check |

---

## 3. Assumptions & Unresolved Business Rules

### 3.1 Inventory Calculation & Semantics
- **Stock Counter Model:** The `Product` entity has `TotalStock`, `PassedStock`, and `FailedStock`.
  - In a standard factory QC workflow, `TotalStock` represents the total batch/registered stock or inventory units available for inspection.
  - When an inspection is confirmed with `QualityStatus == "Passed"`, `PassedStock` is incremented by 1.
  - When confirmed with `QualityStatus == "Failed"`, `FailedStock` is incremented by 1.
  - Invariant: At all times, `PassedStock >= 0`, `FailedStock >= 0`, and `(PassedStock + FailedStock) <= TotalStock`.
  - If `(PassedStock + FailedStock) == TotalStock`, all units in the registered batch have been inspected. If a new inspection is confirmed when `PassedStock + FailedStock >= TotalStock`, the system can either increment `TotalStock` automatically by 1 or return a validation warning. In this implementation, if an inspected unit is confirmed, `TotalStock` will be kept at least equal to `PassedStock + FailedStock` (`TotalStock = Math.Max(TotalStock, PassedStock + FailedStock)`), ensuring data integrity.

### 3.2 Report Lifecycle & States
The `QualityStatus` column in `InspectionReport` can hold the following standardized values:
- `"Passed"`: Product meets all QC standards; no defects detected.
- `"Failed"`: Defect detected exceeding allowable tolerance.
- `"NeedsReview"`: AI is uncertain, image is partially blurry, or confidence score is low (< 0.70). Manual human determination is required before confirmation.
- `"Pending"`: Initial state upon upload before AI processing completes.

### 3.3 Confirmation Business Rules
1. `IsUserConfirmed` is initially `false` when a report is created via `POST /api/inspections/analyze`.
2. A report with `QualityStatus == "NeedsReview"` cannot update inventory until the user explicitly resolves the status to `"Passed"` or `"Failed"` in the confirmation payload.
3. Upon confirmation (`POST /api/inspections/{id}/confirm`), the user may submit optional adjustments (e.g. corrected `DefectCategoryId`, corrected `QualityStatus`, or refined `Notes`).
4. Once `IsUserConfirmed == true`, the report is locked and immutable to prevent audit tampering. Subsequent confirmation calls return HTTP 409 Conflict.

### 3.4 AI Service Failure & Offline Fallback
- If the Gemini API key is not configured, or if the external API returns HTTP 429/500/timeout, the application must:
  - Not crash or expose internal stack traces (graceful degradation).
  - Automatically create an unconfirmed draft inspection report with `QualityStatus: "NeedsReview"`, `ConfidenceScore: 0.0`, `IsAiFallback: true`, and a clear explanatory message, returning **HTTP 200 OK** so the QC inspector can proceed with manual inspection on the web UI without disruption.
  - Never mutate product inventory during analysis.
  - Never fabricate a false "Passed" result.

---

## 4. Non-Functional Requirements (NFRs)

### 4.1 Security
- **API Key Protection:** The `GEMINI_API_KEY` is loaded strictly via ASP.NET Core `IConfiguration` from environment variables, User Secrets, or `appsettings.Development.json`. No secrets are committed to Git.
- **File Upload Security:**
  - Whitelist file extensions: `.jpg`, `.jpeg`, `.png`, `.webp`.
  - Magic byte validation: Verify image header bytes (e.g. `FF D8 FF` for JPEG, `89 50 4E 47` for PNG).
  - Maximum upload size: 10 MB.
  - File renaming: All saved files use cryptographically secure GUIDs (`{guid}_{timestamp}.{ext}`) to completely mitigate directory traversal (`../`) and remote code execution exploits.
- **CORS:** Restrict CORS policy to the local Vite frontend origin (e.g. `http://localhost:5173`, `http://localhost:3000`).

### 4.2 Performance & Responsiveness
- **Async/Await:** All I/O operations (database queries, file streaming, external AI HTTP requests) must be fully asynchronous with `CancellationToken` support.
- **Image Compression/Thumbnails:** Fast loading of image previews in React with responsive caching headers.

### 4.3 UI/UX Standards
- Consistent, modern design system: Tech-forward industrial aesthetics, clear visual indicators (green for Passed, red/amber for Failed, yellow for Needs Review).
- Clear progress animations during AI analysis and comprehensive error banners on network or validation failures.
