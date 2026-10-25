# Full-Stack Implementation Plan & Roadmap

## AI Product Defect Inspection & Quality Control Assistant

---

## 1. Project Directory Structure

```text
d:/GANA/Homework/
├── docs/
│   ├── REQUIREMENTS.md          # Comprehensive requirements traceability & NFRs
│   ├── ERD.md                   # Complete database schema, relationships & seeds
│   ├── WORKFLOW.md              # System architecture, sequence diagrams & state machine
│   ├── API-CONTRACT.md          # Full RESTful API specifications with examples
│   ├── API-TESTING.md           # Curl & PowerShell testing guide for 15 scenarios
│   └── IMPLEMENTATION-PLAN.md   # Phased step-by-step implementation guide
├── backend/
│   ├── AIProductQC.sln          # Visual Studio / .NET Solution
│   ├── AIProductQC.API/         # Presentation & Controller layer
│   │   ├── Controllers/         # ProductsController, InspectionsController, etc.
│   │   ├── DTOs/                # Request & Response Data Transfer Objects
│   │   ├── Middleware/          # GlobalExceptionMiddleware, Error handling
│   │   ├── wwwroot/uploads/     # Controlled static storage for inspection images
│   │   ├── Program.cs           # Service registration, CORS, Swagger, DI
│   │   └── appsettings.json     # Connection strings & Gemini config
│   ├── AIProductQC.Core/        # Domain entities, Enums & Interfaces
│   │   ├── Entities/            # Product, Inspector, DefectCategory, InspectionReport
│   │   ├── Enums/               # QualityStatus, SeverityLevel
│   │   └── Interfaces/          # IGeminiVisionService, IFileStorageService, IInspectionService
│   ├── AIProductQC.Infrastructure/ # Data access, EF Core & external integrations
│   │   ├── Data/                # ApplicationDbContext, Model configurations
│   │   ├── Migrations/          # EF Core Code First Migrations
│   │   ├── Services/            # LocalFileStorageService, GeminiVisionService
│   │   └── Seeds/               # Database seeder for demo data
│   └── AIProductQC.Tests/       # Unit & integration tests for confirmation & transactions
├── frontend/
│   ├── src/
│   │   ├── components/          # Header, StatCard, Badge, ImageDropzone, Modal, etc.
│   │   ├── pages/               # Dashboard, UploadInspect, PreviewConfirm, History, Detail
│   │   ├── services/            # api.js, inspectionService.js, productService.js
│   │   ├── styles/              # index.css, theme design system tokens
│   │   ├── App.jsx              # Routes & navigation layout
│   │   └── main.jsx             # React root initialization
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   └── .env.example
├── sample_images/               # Curated test images (good, scratch, crack, blurry, etc.)
├── AGENT_INSTRUCTIONS.md        # Master agent requirements
└── README.md                    # Project presentation & run guide
```

---

## 2. Phased Execution Roadmap

### Phase 1 — Analysis, Architecture & Design (CURRENT PHASE)
- [x] Read and analyze `AGENT_INSTRUCTIONS.md` and `docs/Homework.pdf`.
- [x] Create comprehensive `docs/REQUIREMENTS.md` with traceability matrix.
- [x] Create visual `docs/ERD.md` with Mermaid diagram, exact SQL types, delete behaviors, and seed data.
- [x] Create `docs/WORKFLOW.md` detailing analyze, preview, confirmation, and error sequences.
- [x] Create `docs/API-CONTRACT.md` detailing all 10 endpoints, status codes, and JSON schemas.
- [x] Create `docs/API-TESTING.md` covering all 15 testing scenarios.
- [x] Create `docs/IMPLEMENTATION-PLAN.md` with detailed tasks and verification gates.
- [x] Create `.agents/workspace/plan.md` conforming to the Multi-Agent Development System.

---

### Phase 2 — Backend Foundation
1. **Solution & Project Setup:**
   - Create solution `AIProductQC.sln`.
   - Create projects:
     - `AIProductQC.Core` (Class Library, .NET 8 / 10)
     - `AIProductQC.Infrastructure` (Class Library, .NET 8 / 10)
     - `AIProductQC.API` (ASP.NET Core Web API, .NET 8 / 10)
     - `AIProductQC.Tests` (xUnit test project)
   - Establish project references: API -> Infrastructure & Core; Infrastructure -> Core.
2. **Core Domain Implementation:**
   - Define domain entities: `Product`, `Inspector`, `DefectCategory`, `InspectionReport`.
   - Define enums: `QualityStatus` (`Passed`, `Failed`, `NeedsReview`, `Pending`), `SeverityLevel` (`None`, `Low`, `Medium`, `High`, `Critical`).
   - Define repository and service contracts: `IFileStorageService`, `IGeminiVisionService`, `IInspectionService`.
3. **Infrastructure & EF Core Configuration:**
   - Install EF Core packages (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`).
   - Implement `ApplicationDbContext` with Fluent API mappings:
     - Exact string max lengths, non-null columns.
     - `DeleteBehavior.Restrict` on all foreign keys.
     - Unique indexes on `Inspector.Email` and `DefectCategory.CategoryName`.
     - `RowVersion` concurrency token on `InspectionReport`.
4. **Database Migration & Seeding:**
   - Configure connection string for `(localdb)\mssqllocaldb`.
   - Add initial migration: `dotnet ef migrations add InitialCreate --project AIProductQC.Infrastructure --startup-project AIProductQC.API`.
   - Implement `DatabaseSeeder` to populate standard products, inspectors, and defect categories (including "Không có lỗi (No Defect)").
   - Verify SQL Server tables and seed records via command-line query.

---

### Phase 3 — Backend Features & AI Integration
1. **File Storage Service:**
   - Implement `LocalFileStorageService`:
     - Secure path resolution inside `AIProductQC.API/wwwroot/uploads`.
     - File extension whitelisting (`.jpg`, `.jpeg`, `.png`, `.webp`).
     - Magic bytes verification (JPEG, PNG, WEBP file headers).
     - Unique GUID filename generation (`qc_{guid}{ext}`).
2. **Gemini Vision AI Integration:**
   - Implement `GeminiVisionService`:
     - Load API key from configuration (`Gemini:ApiKey` or `GEMINI_API_KEY` env var).
     - Construct HTTP request to Google Generative Language API endpoint (`models/gemini-3.5-flash:generateContent`).
     - Configure request body with Base64 inline image data and system prompt demanding structured JSON.
     - Prompt instructs AI to produce: `categoryName`, `defectDescription` (in Vietnamese), `qualityStatus`, and `confidenceScore`.
     - Resilient error handling: catch rate limits (429), timeouts, invalid keys; fallback to `NeedsReview` draft without crashing.
     - Unit test for parsing valid and malformed AI JSON.
3. **Controllers & Endpoints Implementation:**
   - `ProductsController`: `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`.
   - `InspectorsController`: `GET /api/inspectors`.
   - `DefectCategoriesController`: `GET /api/defect-categories`.
   - `InspectionsController`:
     - `POST /api/inspections/analyze`: Multipart upload, file validation, storage, Gemini call, unconfirmed report creation, response DTO (Inventory strictly untouched!).
     - `POST /api/inspections/{id}/confirm`: Atomic `IDbContextTransaction`, duplicate check, inventory calculation (`PassedStock` / `FailedStock`), commit/rollback.
     - `GET /api/inspections`: Filterable and paginated report list.
     - `GET /api/inspections/{id}`: Detailed inspection view.
   - `DashboardController`:
     - `GET /api/dashboard/summary`: Aggregated metrics, pass rates, defect category distribution.
4. **Middleware, CORS & Swagger:**
   - Register `GlobalExceptionMiddleware` for standardized RFC 7807 error responses.
   - Configure CORS allowing frontend origins (`http://localhost:5173`).
   - Enable Swagger documentation with XML comments and schema definitions.
   - Configure static file serving for `wwwroot/uploads`.

---

### Phase 4 — Frontend Development (React + Vite)
1. **Project Scaffolding & Styling System:**
   - Initialize React Vite application in `frontend/`.
   - Install dependencies: `axios`, `react-router-dom`, `lucide-react` (modern icons), `canvas-confetti` (for inspection pass celebration).
   - Establish modern design system tokens in `src/styles/index.css`:
     - Slate/industrial dark accents with high-contrast vibrant accents (Emerald for Passed, Rose for Failed, Amber for Review).
     - Typography, cards, gradients, micro-animations, glassmorphism modals.
2. **API Client & Services:**
   - Create centralized Axios instance in `src/services/api.js` with base URL and timeout handling.
   - Implement service modules: `productService.js`, `inspectorService.js`, `inspectionService.js`, `dashboardService.js`.
3. **Core Pages Implementation:**
   - **QC Dashboard (`/`):** Summary metrics cards, pass rate indicator, defect distribution bar, recent inspection table, quick action buttons.
   - **Upload & Inspect (`/inspect`):** Product selector, Inspector selector, interactive drag-and-drop image dropzone with live preview, "Phân Tích AI" button with animated scanner pulse.
   - **Preview & Confirm (`/inspect/preview/:id`):** Split-screen layout: inspection image on left; AI findings on right (Category badge, Vietnamese diagnosis, confidence meter, quality status). Editable overrides for status and category, "Xác Nhận & Cập Nhật Tồn Kho" button, cancel/retry action.
   - **Inspection History (`/history`):** Search by product, filter by quality status, date range filter, responsive data table with status badges and detail buttons.
   - **Inspection Detail (`/history/:id`):** Full report inspection, zoomable image, audit trail, confirmation timestamp, inspector details.
4. **Feedback & States:**
   - Skeleton loaders during API calls.
   - Empty state illustrations when no records exist.
   - Error banners with retry buttons on network failure.

---

### Phase 5 — Integration & Quality Assurance
1. **End-to-End Verification:**
   - Launch backend (`dotnet run`) and frontend (`npm run dev`).
   - Verify CORS, static image serving from `http://localhost:5000/uploads/...`.
2. **Execution of 15 Required Test Scenarios:**
   - Run automated and manual tests as cataloged in `docs/API-TESTING.md`.
   - Verify that inventory is **never** updated before confirmation.
   - Verify that double-clicking "Confirm" returns HTTP 409 and does not double-count inventory.
   - Verify atomic rollback on simulated database failure.
   - Verify graceful handling when Gemini API key is missing or simulated offline.

---

### Phase 6 — Documentation, Demo & Delivery
1. **README.md Compilation:**
   - Project overview, architecture diagrams, prerequisites (.NET SDK, Node.js, SQL Server).
   - Step-by-step setup and launch instructions for backend and frontend.
   - Environment variables setup (`GEMINI_API_KEY`).
   - Database migration commands.
   - API endpoints reference table and test results summary.
2. **Presentation Demo Plan:**
   - Prepare a 3-minute scripted presentation demonstrating:
     1. Dashboard initial stats.
     2. Uploading a defective product image -> AI detection in Vietnamese.
     3. Explaining zero inventory update before confirmation.
     4. Confirming report -> Atomic stock update reflected in Dashboard.
     5. Viewing report history and details.
3. **Git Repository Finalization:**
   - Check `.gitignore` (ignore `bin/`, `obj/`, `node_modules/`, `wwwroot/uploads/*.jpg`, `.env`).
   - Verify no sensitive keys are tracked.
