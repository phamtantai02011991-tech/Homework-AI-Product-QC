using System;
using AIProductQC.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIProductQC.Infrastructure.Data;

/// <summary>
/// Database context for AI Product Defect Inspection & Quality Control Assistant.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inspector> Inspectors => Set<Inspector>();
    public DbSet<DefectCategory> DefectCategories => Set<DefectCategory>();
    public DbSet<InspectionReport> InspectionReports => Set<InspectionReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.ProductId)
                  .UseIdentityColumn();

            entity.Property(p => p.ProductName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(p => p.Category)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(p => p.TotalStock)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(p => p.PassedStock)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(p => p.FailedStock)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(p => p.CreatedAt)
                  .IsRequired();

            entity.HasIndex(p => p.ProductName);
        });

        // Inspector Configuration
        modelBuilder.Entity<Inspector>(entity =>
        {
            entity.ToTable("Inspectors");
            entity.HasKey(i => i.InspectorId);

            entity.Property(i => i.InspectorId)
                  .UseIdentityColumn();

            entity.Property(i => i.InspectorName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(i => i.Department)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(i => i.Email)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(i => i.Email)
                  .IsUnique();
        });

        // DefectCategory Configuration
        modelBuilder.Entity<DefectCategory>(entity =>
        {
            entity.ToTable("DefectCategories");
            entity.HasKey(c => c.DefectCategoryId);

            entity.Property(c => c.DefectCategoryId)
                  .UseIdentityColumn();

            entity.Property(c => c.CategoryName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.SeverityLevel)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(c => c.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.HasIndex(c => c.CategoryName)
                  .IsUnique();
        });

        // InspectionReport Configuration
        modelBuilder.Entity<InspectionReport>(entity =>
        {
            entity.ToTable("InspectionReports");
            entity.HasKey(r => r.ReportId);

            entity.Property(r => r.ReportId)
                  .UseIdentityColumn();

            entity.Property(r => r.ImageUrl)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(r => r.DefectDescription)
                  .IsRequired();

            entity.Property(r => r.QualityStatus)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(r => r.ConfidenceScore)
                  .IsRequired();

            entity.Property(r => r.IsUserConfirmed)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(r => r.CreatedAt)
                  .IsRequired();

            entity.Property(r => r.RowVersion)
                  .IsRowVersion();

            // Product 1:N InspectionReport
            entity.HasOne(r => r.Product)
                  .WithMany(p => p.InspectionReports)
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Inspector 1:N InspectionReport
            entity.HasOne(r => r.Inspector)
                  .WithMany(i => i.InspectionReports)
                  .HasForeignKey(r => r.InspectorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // DefectCategory 1:N InspectionReport
            entity.HasOne(r => r.DefectCategory)
                  .WithMany(c => c.InspectionReports)
                  .HasForeignKey(r => r.DefectCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Performance indexes
            entity.HasIndex(r => new { r.ProductId, r.CreatedAt });
            entity.HasIndex(r => r.QualityStatus);
            entity.HasIndex(r => r.IsUserConfirmed);
        });

        // Seed initial data in English
        SeedInitialData(modelBuilder);
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        // 1. Defect Categories (must include No Defect for foreign key integrity)
        modelBuilder.Entity<DefectCategory>().HasData(
            new DefectCategory
            {
                DefectCategoryId = 1,
                CategoryName = "Không có lỗi (No Defect)",
                SeverityLevel = "None",
                Description = "Sản phẩm đạt toàn bộ tiêu chuẩn chất lượng, bề mặt hoàn thiện không tì vết."
            },
            new DefectCategory
            {
                DefectCategoryId = 2,
                CategoryName = "Trầy xước bề mặt (Surface Scratch)",
                SeverityLevel = "Low",
                Description = "Vết trầy xước hoặc xước mờ trên bề mặt, không ảnh hưởng đến độ bền cơ học."
            },
            new DefectCategory
            {
                DefectCategoryId = 3,
                CategoryName = "Nứt gãy kết cấu (Structural Crack)",
                SeverityLevel = "High",
                Description = "Vết nứt vỡ hoặc rạn kết cấu làm suy giảm độ bền cơ học."
            },
            new DefectCategory
            {
                DefectCategoryId = 4,
                CategoryName = "Móp méo / Biến dạng (Dent / Deformation)",
                SeverityLevel = "Medium",
                Description = "Vết lõm, cong vênh vỏ hoặc thân sản phẩm do gia công hoặc va đập."
            },
            new DefectCategory
            {
                DefectCategoryId = 5,
                CategoryName = "Ăn mòn / Đổi màu (Discoloration / Corrosion)",
                SeverityLevel = "Medium",
                Description = "Ố màu bề mặt, oxy hóa, gỉ sét hoặc đổi màu lớp phủ bảo vệ."
            },
            new DefectCategory
            {
                DefectCategoryId = 6,
                CategoryName = "Lệch khớp lắp ráp (Assembly Misalignment)",
                SeverityLevel = "High",
                Description = "Hở mối nối, khe hở sai lệch hoặc thiếu chốt vít linh kiện lắp ráp."
            }
        );

        // 2. Inspectors
        modelBuilder.Entity<Inspector>().HasData(
            new Inspector
            {
                InspectorId = 1,
                InspectorName = "Nguyễn Văn An",
                Department = "QC Điện Tử & Màn Hình",
                Email = "an.nguyen@factoryqc.vn"
            },
            new Inspector
            {
                InspectorId = 2,
                InspectorName = "Trần Thị Bích",
                Department = "QC Gia Công Cơ Khí",
                Email = "bich.tran@factoryqc.vn"
            },
            new Inspector
            {
                InspectorId = 3,
                InspectorName = "Lê Hoàng Nam",
                Department = "QA Đóng Gói & Xuất Xưởng",
                Email = "nam.le@factoryqc.vn"
            }
        );

        // 3. Products
        var baseDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = 1,
                ProductName = "Màn hình OLED 6.7 inch FHD+",
                Category = "Linh kiện điện tử",
                TotalStock = 500,
                PassedStock = 450,
                FailedStock = 12,
                CreatedAt = baseDate.AddDays(1)
            },
            new Product
            {
                ProductId = 2,
                ProductName = "Vỏ nhôm Anode động cơ servo",
                Category = "Cơ khí chính xác",
                TotalStock = 250,
                PassedStock = 210,
                FailedStock = 18,
                CreatedAt = baseDate.AddDays(5)
            },
            new Product
            {
                ProductId = 3,
                ProductName = "Bo mạch chủ IoT Gateway v2.1",
                Category = "Linh kiện điện tử",
                TotalStock = 180,
                PassedStock = 165,
                FailedStock = 5,
                CreatedAt = baseDate.AddDays(10)
            },
            new Product
            {
                ProductId = 4,
                ProductName = "Bộ gốm cách điện chịu áp cao",
                Category = "Vật liệu cách điện",
                TotalStock = 300,
                PassedStock = 280,
                FailedStock = 8,
                CreatedAt = baseDate.AddDays(15)
            },
            new Product
            {
                ProductId = 5,
                ProductName = "Cụm camera cảm biến hồng ngoại",
                Category = "Quang học chính xác",
                TotalStock = 120,
                PassedStock = 98,
                FailedStock = 6,
                CreatedAt = baseDate.AddDays(20)
            }
        );
    }
}
