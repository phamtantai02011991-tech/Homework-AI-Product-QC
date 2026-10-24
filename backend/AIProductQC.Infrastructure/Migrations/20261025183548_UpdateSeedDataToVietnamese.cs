using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIProductQC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedDataToVietnamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 1,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Không có lỗi (No Defect)", "Sản phẩm đạt toàn bộ tiêu chuẩn chất lượng, bề mặt hoàn thiện không tì vết." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 2,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Trầy xước bề mặt (Surface Scratch)", "Vết trầy xước hoặc xước mờ trên bề mặt, không ảnh hưởng đến độ bền cơ học." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 3,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Nứt gãy kết cấu (Structural Crack)", "Vết nứt vỡ hoặc rạn kết cấu làm suy giảm độ bền cơ học." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 4,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Móp méo / Biến dạng (Dent / Deformation)", "Vết lõm, cong vênh vỏ hoặc thân sản phẩm do gia công hoặc va đập." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 5,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Ăn mòn / Đổi màu (Discoloration / Corrosion)", "Ố màu bề mặt, oxy hóa, gỉ sét hoặc đổi màu lớp phủ bảo vệ." });

            migrationBuilder.UpdateData(
                table: "DefectCategories",
                keyColumn: "DefectCategoryId",
                keyValue: 6,
                columns: new[] { "CategoryName", "Description" },
                values: new object[] { "Lệch khớp lắp ráp (Assembly Misalignment)", "Hở mối nối, khe hở sai lệch hoặc thiếu chốt vít linh kiện lắp ráp." });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
