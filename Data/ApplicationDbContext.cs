using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Models;

namespace StoreAPI.Data;
public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // เพิ่มส่วนนี้เข้าไป
        base.OnModelCreating(modelBuilder);

         modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName)
                .HasMaxLength(64)
                .IsUnicode(false);
            
            // Seed data
            entity.HasData(
                new Category { CategoryId = 1, CategoryName = "Mobile", CategoryStatus = 1},
                new Category { CategoryId = 2, CategoryName = "Tablet", CategoryStatus = 1},
                new Category { CategoryId = 3, CategoryName = "Smart Watch", CategoryStatus = 1},
                new Category { CategoryId = 4, CategoryName = "Laptop", CategoryStatus = 1}
            );
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId); // Define primary key
            entity.Property(e => e.CreatedDate).HasColumnType("timestamptz"); // Use timestamptz for PostgreSQL
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamptz"); // Use timestamptz for PostgreSQL
            entity.Property(e => e.ProductName)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.ProductPicture)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            // Seed data
            entity.HasData(
                new Product
                {
                    ProductId = 1,
                    ProductName = "iPhone 13 Pro Max",
                    UnitPrice = 55000,
                    UnitinStock = 3,
                    ProductPicture = "https://www.mxphone.com/wp-content/uploads/2021/04/41117-79579-210401-iPhone12ProMax-xl-1200x675.jpg",
                    CategoryId = 1,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc),
                    ModifiedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "iPad Pro 2021",
                    UnitPrice = 18500,
                    UnitinStock = 10,
                    ProductPicture = "https://cdn.siamphone.com/spec/apple/images/ipad_pro_12.9%E2%80%91inch/com_1.jpg",
                    CategoryId = 2,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc),
                    ModifiedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Airpods Pro",
                    UnitPrice = 12000,
                    UnitinStock = 5,
                    ProductPicture = "https://www.avtechguide.com/wp-content/uploads/2020/11/leaked-apple-airpods-pro-generation2-info_01-800x445.jpg",
                    CategoryId = 3,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc),
                    ModifiedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Macbook Pro M1",
                    UnitPrice = 45000,
                    UnitinStock = 10,
                    ProductPicture = "https://cdn.mos.cms.futurecdn.net/iYCQTPgBSdDmkYESfPkunh.jpg",
                    CategoryId = 4,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc),
                    ModifiedDate = DateTime.SpecifyKind(DateTime.Parse("2021-11-22T00:00:00"), DateTimeKind.Utc)
                }
            );
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
