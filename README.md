# AI Product Defect Inspection & Quality Control Assistant

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-blue.svg)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-8.3-yellow.svg)](https://vite.dev/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-red.svg)](https://microsoft.com/sql-server)
[![Gemini](https://img.shields.io/badge/AI-Google_Gemini_Vision-emerald.svg)](https://ai.google.dev/)

An enterprise-grade, educational full-stack web application designed for manufacturing and warehouse quality control (QC). The system leverages **Google Gemini Vision Multimodal AI** to inspect product images, detect surface and structural defects, classify defect categories, generate localized Vietnamese diagnostic descriptions, and atomically update product inventory counters only after explicit human confirmation.

---

## 1. System Architecture & Tech Stack

```text
React.js + Vite (Frontend)
   │ (REST / Axios / JSON)
   ▼
ASP.NET Core Web API (Backend)
   ├─► Local File Storage Service (wwwroot/uploads)
   ├─► Google Gemini Vision API (Multimodal AI Vision)
   └─► Entity Framework Core (SQL Server LocalDB / Transactions)
```

- **Frontend:** React.js, Vite, JavaScript, Axios, React Router, Canvas Confetti, Lucide Icons, Vanilla CSS Design System (Dark Industrial Glassmorphism).
- **Backend:** ASP.NET Core Web API, C#, Entity Framework Core (Code First), SQL Server LocalDB, Swagger / OpenAPI.
- **AI Engine:** Google Gemini Multimodal Vision API (`gemini-3.5-flash`), with structured JSON output schema and localized Vietnamese defect analysis prompts.
- **Architecture Layers:**
  - `AIProductQC.Core`: Domain Entities, Enums, Service Interfaces.
  - `AIProductQC.Infrastructure`: EF Core `ApplicationDbContext`, Fluent API Mappings, Migrations, Local File Storage, Gemini Vision Service.
  - `AIProductQC.API`: RESTful Controllers, DTOs, Global Exception Middleware, Static File Hosting, Swagger UI.
  - `AIProductQC.Tests`: xUnit Test Suite for image validation and confirmation business logic.

---

## 2. Key Business Rules & Guarantees

1. **Zero-Inventory-Mutation Guarantee:** During the AI image analysis phase (`POST /api/inspections/analyze`), product inventory counts are **strictly untouched**. The report is saved as an unconfirmed draft (`IsUserConfirmed = false`).
2. **Atomic Confirmation Transactions:** Inventory updates (`POST /api/inspections/{id}/confirm`) are wrapped in an `IDbContextTransaction`. If any operation fails, the entire transaction rolls back.
3. **Duplicate Confirmation Prevention:** Confirmed reports are immutable. Re-confirming an already confirmed report returns `HTTP 409 Conflict`, preventing double-counting stock.
4. **Manual Review Enforcement:** Reports in `NeedsReview` state (e.g. blurry/unclear images) cannot update inventory until an inspector explicitly chooses `Passed` or `Failed`.
5. **Secure File Uploads:** Uploaded images undergo magic-byte signature validation, file size limits (10MB), and are renamed with cryptographically secure GUIDs (`qc_{guid}.jpg`) to prevent directory traversal attacks.

---

## 3. Database Schema & Entities

The database schema implements the four mandatory entities:

- **Product:** `ProductId` (PK), `ProductName`, `Category`, `TotalStock`, `PassedStock`, `FailedStock`, `CreatedAt`.
- **Inspector:** `InspectorId` (PK), `InspectorName`, `Department`, `Email` (Unique).
- **DefectCategory:** `DefectCategoryId` (PK), `CategoryName` (Unique), `SeverityLevel`, `Description`.
  - Includes mandatory seed: `DefectCategoryId = 1` (`"Không có lỗi (No Defect)"`, Severity: `"None"`).
- **InspectionReport:** `ReportId` (PK), `ProductId` (FK), `InspectorId` (FK), `DefectCategoryId` (FK), `ImageUrl`, `DefectDescription`, `QualityStatus`, `ConfidenceScore`, `IsUserConfirmed`, `CreatedAt`, `RowVersion` (Concurrency token).

Relationships use `DeleteBehavior.Restrict` to preserve audit compliance records.

---

## 4. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 8 / 9 SDK).
- [Node.js](https://nodejs.org/) (v18.0 or newer).
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (`mssqllocaldb`).
- Google Gemini API Key (optional for live AI calls; offline fallback is included).

---

## 5. Quick Start & Setup Instructions

### 5.1 Clone & Navigate
```bash
git clone <your-repo-url>
cd Homework
```

### 5.2 Configure Google Gemini API Key (Secure Secret Management)
> **Security Notice:** Do NOT commit your API key to source control. If your key was previously exposed, please **revoke** it immediately on [Google AI Studio](https://aistudio.google.com/app/apikey) and generate a new key.

Configure your API key securely using **.NET User Secrets** (recommended for local development) or an environment variable:

```bash
# Option A: .NET User Secrets (Recommended for local dev)
dotnet user-secrets set "Gemini:ApiKey" "<YOUR_NEW_GEMINI_API_KEY>" --project backend/AIProductQC.API

# Option B: Environment Variable (Windows PowerShell)
$env:GEMINI_API_KEY="<YOUR_NEW_GEMINI_API_KEY>"

# Option C: Environment Variable (Windows CMD)
set GEMINI_API_KEY=<YOUR_NEW_GEMINI_API_KEY>
```

*Note: If no API key is provided, the backend automatically activates an offline fallback mode (`QualityStatus: "NeedsReview"`, with manual inspection enabled) without crashing.*

### 5.3 Backend Database Migration & Run
Ensure SQL Server LocalDB is started:
```bash
sqllocaldb start MSSQLLocalDB
```

Update the database and launch the API:
```bash
cd backend

# Apply EF Core Migrations and Seed Data
dotnet ef database update --project AIProductQC.Infrastructure --startup-project AIProductQC.API

# Run Web API (default URL: http://localhost:5000)
dotnet run --project AIProductQC.API --urls "http://localhost:5000"
```

Once started, access Swagger documentation at: **`http://localhost:5000/swagger`**

### 5.4 Frontend Setup & Run
Open a new terminal tab:
```bash
cd frontend

# Install dependencies (axios, react-router-dom, lucide-react, canvas-confetti)
npm install

# Start Vite Development Server
npm run dev
```

Open your browser at: **`http://localhost:5173`**

---

## 6. RESTful API Endpoints Reference

| HTTP Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/api/products` | List all products with current stock counts |
| `GET` | `/api/products/{id}` | Get product details and pass rate percentage |
| `POST` | `/api/products` | Register a new product batch |
| `GET` | `/api/inspectors` | List authorized QC inspectors |
| `GET` | `/api/defect-categories` | List defect typology categories |
| `GET` | `/api/inspections` | List inspection reports with filtering & pagination |
| `GET` | `/api/inspections/{id}` | Get detailed report with product and inspector info |
| `POST` | `/api/inspections/analyze` | Upload image, invoke Gemini AI, save unconfirmed draft |
| `POST` | `/api/inspections/{id}/confirm` | Confirm report and atomically update inventory |
| `GET` | `/api/dashboard/summary` | Aggregate dashboard KPIs and defect statistics |

---

## 7. Automated Testing & Verification

Run the automated test suite covering image validation and transaction confirmation logic:

```bash
cd backend
dotnet test AIProductQC.Tests/AIProductQC.Tests.csproj
```

### Test Results Summary:
- **Total Tests:** 11
- **Passed:** 11 (100% Pass Rate)
- **Scenarios Verified:**
  - Valid JPEG magic byte signature verification.
  - Valid PNG magic byte signature verification.
  - File extension whitelist enforcement (`.jpg, .png, .webp`).
  - Executable/script upload rejection (`.exe`).
  - File size limit enforcement (> 10MB rejection).
  - Empty file upload rejection.
  - Atomic confirmation of Passed report (`PassedStock` increments by 1).
  - Atomic confirmation of Failed report (`FailedStock` increments by 1).
  - Prevention of duplicate confirmation (HTTP 409 Conflict).
  - Rejection of unconfirmed `NeedsReview` status without QC decision (HTTP 422).

---

## 8. Presentation Demo Script (3-Minute Walkthrough)

1. **Step 1 — Dashboard Overview (`/`):**
   - Showcase summary KPI cards: Total Products, Inspected Count, Passed/Failed Stock, Pass Rate.
   - Point out Defect Distribution breakdown and explain how data updates in real-time.
2. **Step 2 — Upload & AI Analysis (`/inspect`):**
   - Select product (e.g. `Màn hình OLED 6.7 inch FHD+`) and inspector (`Nguyễn Văn An`).
   - Drag and drop `sample_images/02_minor_defect_scratch.jpg`.
   - Click **"Start AI Defect Analysis"** (*Bắt đầu phân tích bằng AI*). Observe the laser scanning animation and upload progress bar.
   - Explain to the audience: *Notice that at this point, product inventory is strictly unchanged!*
3. **Step 3 — Review & Confirmation (`/inspect/preview/:id`):**
   - Review AI diagnosis: Gemini detects the scratch, maps it to `"Trầy xước bề mặt (Surface Scratch)"`, and generates a Vietnamese description.
   - Point out that QC can adjust the findings or approve them.
   - Click **"Confirm & Update Inventory"** (*Xác nhận & Cập nhật tồn kho*).
   - Watch the celebratory notification: `FailedStock` increments by 1 atomically.
   - Show that clicking confirm a second time is blocked (HTTP 409 guard).
4. **Step 4 — Audit & History (`/history`):**
   - Filter by status `"Failed"` or product name to view the persistent audit log.
   - Click into the inspection detail page to inspect the high-resolution image and complete timestamped record.

---

## 9. Deliverables & Documentation Index

- [docs/REQUIREMENTS.md](file:///d:/GANA/Homework/docs/REQUIREMENTS.md): Requirements Traceability Matrix & Non-Functional Requirements.
- [docs/ERD.md](file:///d:/GANA/Homework/docs/ERD.md): Entity Relationship Diagram, schema types, indexes, and seed records.
- [docs/WORKFLOW.md](file:///d:/GANA/Homework/docs/WORKFLOW.md): Sequence diagrams matching page 3 of Homework.pdf and state machine.
- [docs/API-CONTRACT.md](file:///d:/GANA/Homework/docs/API-CONTRACT.md): Full REST API contracts with JSON schemas and status codes.
- [docs/API-TESTING.md](file:///d:/GANA/Homework/docs/API-TESTING.md): cURL & PowerShell scripts for the 15 mandatory test scenarios.
- [docs/IMPLEMENTATION-PLAN.md](file:///d:/GANA/Homework/docs/IMPLEMENTATION-PLAN.md): Full engineering roadmap and execution gates.
