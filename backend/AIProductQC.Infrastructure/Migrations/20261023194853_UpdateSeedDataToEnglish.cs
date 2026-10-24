using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIProductQC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedDataToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 1,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "No Defect", "Product meets all quality specifications with flawless surface finish." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 2,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Surface Scratch", "Minor surface scratch or abrasion not impacting structural integrity." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 3,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Structural Crack", "Fracture or structural crack compromising mechanical strength." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 4,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Dent / Deformation", "Casing or body indentation or warpage from machining or handling." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 5,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Discoloration / Corrosion", "Surface staining, oxidation, tarnishing or coating discoloration." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 6,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Assembly Misalignment", "Misaligned seams, gaps, or missing assembly fasteners." });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 1,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "Electronics QC", "alex.nguyen@factoryqc.com", "Alex Nguyen" });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 2,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "Mechanical QC", "sarah.tran@factoryqc.com", "Sarah Tran" });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 3,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "Packaging QA", "david.le@factoryqc.com", "David Le" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Electronics", "OLED Display Module 6.7\"" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Precision Mechanics", "Servo Motor Aluminum Housing" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Electronics", "IoT Gateway Mainboard v2.1" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Insulation Materials", "High-Voltage Ceramic Insulator" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Precision Optics", "Infrared Sensor Camera Unit" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 1,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Không có lỗi (No Defect)", "Sản phẩm đạt chuẩn, bề mặt hoàn thiện tốt, không phát hiện khuyết tật ngoại quan." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 2,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Trầy xước bề mặt (Surface Scratch)", "Vết xước mảnh hoặc cọ xát trên bề mặt vỏ hoặc linh kiện, không ảnh hưởng kết cấu." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 3,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Nứt vỡ cấu trúc (Structural Crack)", "Đường nứt hoặc rạn vỡ thân sản phẩm làm giảm độ bền cơ học và an toàn." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 4,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Móp méo biến dạng (Dent / Deformation)", "Vỏ hoặc thân kim loại/nhựa bị lõm, cong vênh do va đập trong gia công hoặc lưu kho." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 5,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Biến màu & Ăn mòn (Discoloration / Corrosion)", "Ố vàng, loang màu sơn, oxy hóa hoặc rỉ sét bề mặt do độ ẩm môi trường." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 6,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Lắp ráp sai lệch (Assembly Misalignment)", "Khe hở không đồng đều, khớp nối bị lệch hoặc thiếu ốc vít cố định." });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 1,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "QC Điện Tử & Màn Hình", "an.nguyen@factoryqc.vn", "Nguyễn Văn An" });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 2,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "QC Gia Công Cơ Khí", "bich.tran@factoryqc.vn", "Trần Thị Bích" });

            migrationBuilder.UpdateData(
                table: "Inspectors",
                keyColumn: "InspectorId",
                keyValue: 3,
                columns: new[] { "Department", "Email", "InspectorName" },
                values: new object[] { "QA Đóng Gói & Xuất Xưởng", "nam.le@factoryqc.vn", "Lê Hoàng Nam" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Linh kiện điện tử", "Màn hình OLED 6.7 inch FHD+" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Cơ khí chính xác", "Vỏ nhôm Anode động cơ servo" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Linh kiện điện tử", "Bo mạch chủ IoT Gateway v2.1" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Vật liệu cách điện", "Bộ gốm cách điện chịu áp cao" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "Category", "ProductName" },
                values: new object[] { "Quang học chính xác", "Cụm camera cảm biến hồng ngoại" });
        }
    }
}
