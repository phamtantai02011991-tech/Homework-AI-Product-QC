using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AIProductQC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefectCategories",
                columns: table => new
                {
                    DefectCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectCategories", x => x.DefectCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Inspectors",
                columns: table => new
                {
                    InspectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspectors", x => x.InspectorId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PassedStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FailedStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "InspectionReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    InspectorId = table.Column<int>(type: "int", nullable: false),
                    DefectCategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    IsUserConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_InspectionReports_DefectCategories_DefectCategoryId",
                        column: x => x.DefectCategoryId,
                        principalTable: "DefectCategories",
                        principalColumn: "DefectCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Inspectors_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Inspectors",
                        principalColumn: "InspectorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DefectCategories",
                columns: new[] { "DefectCategoryId", "CategoryName", "Description", "SeverityLevel" },
                values: new object[,]
                {
                    { 1, "Không có lỗi (No Defect)", "Sản phẩm đạt chuẩn, bề mặt hoàn thiện tốt, không phát hiện khuyết tật ngoại quan.", "None" },
                    { 2, "Trầy xước bề mặt (Surface Scratch)", "Vết xước mảnh hoặc cọ xát trên bề mặt vỏ hoặc linh kiện, không ảnh hưởng kết cấu.", "Low" },
                    { 3, "Nứt vỡ cấu trúc (Structural Crack)", "Đường nứt hoặc rạn vỡ thân sản phẩm làm giảm độ bền cơ học và an toàn.", "High" },
                    { 4, "Móp méo biến dạng (Dent / Deformation)", "Vỏ hoặc thân kim loại/nhựa bị lõm, cong vênh do va đập trong gia công hoặc lưu kho.", "Medium" },
                    { 5, "Biến màu & Ăn mòn (Discoloration / Corrosion)", "Ố vàng, loang màu sơn, oxy hóa hoặc rỉ sét bề mặt do độ ẩm môi trường.", "Medium" },
                    { 6, "Lắp ráp sai lệch (Assembly Misalignment)", "Khe hở không đồng đều, khớp nối bị lệch hoặc thiếu ốc vít cố định.", "High" }
                });

            migrationBuilder.InsertData(
                table: "Inspectors",
                columns: new[] { "InspectorId", "Department", "Email", "InspectorName" },
                values: new object[,]
                {
                    { 1, "QC Điện Tử & Màn Hình", "an.nguyen@factoryqc.vn", "Nguyễn Văn An" },
                    { 2, "QC Gia Công Cơ Khí", "bich.tran@factoryqc.vn", "Trần Thị Bích" },
                    { 3, "QA Đóng Gói & Xuất Xưởng", "nam.le@factoryqc.vn", "Lê Hoàng Nam" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Category", "CreatedAt", "FailedStock", "PassedStock", "ProductName", "TotalStock" },
                values: new object[,]
                {
                    { 1, "Linh kiện điện tử", new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc), 12, 450, "Màn hình OLED 6.7 inch FHD+", 500 },
                    { 2, "Cơ khí chính xác", new DateTime(2026, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc), 18, 210, "Vỏ nhôm Anode động cơ servo", 250 },
                    { 3, "Linh kiện điện tử", new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), 5, 165, "Bo mạch chủ IoT Gateway v2.1", 180 },
                    { 4, "Vật liệu cách điện", new DateTime(2026, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), 8, 280, "Bộ gốm cách điện chịu áp cao", 300 },
                    { 5, "Quang học chính xác", new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 6, 98, "Cụm camera cảm biến hồng ngoại", 120 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefectCategories_CategoryName",
                table: "DefectCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_DefectCategoryId",
                table: "InspectionReports",
                column: "DefectCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_InspectorId",
                table: "InspectionReports",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_IsUserConfirmed",
                table: "InspectionReports",
                column: "IsUserConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_ProductId_CreatedAt",
                table: "InspectionReports",
                columns: new[] { "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_QualityStatus",
                table: "InspectionReports",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Inspectors_Email",
                table: "Inspectors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionReports");

            migrationBuilder.DropTable(
                name: "DefectCategories");

            migrationBuilder.DropTable(
                name: "Inspectors");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
