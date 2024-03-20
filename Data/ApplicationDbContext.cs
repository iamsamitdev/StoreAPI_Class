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

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<product> products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // เพิ่มส่วนนี้เข้าไป
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.category_id).HasName("categories_pkey");

            entity.Property(e => e.category_name).HasMaxLength(128);
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.product_id).HasName("products_pkey");

            entity.Property(e => e.created_date).HasColumnType("timestamp without time zone");
            entity.Property(e => e.modified_date).HasColumnType("timestamp without time zone");
            entity.Property(e => e.product_name).HasMaxLength(50);
            entity.Property(e => e.product_picture).HasMaxLength(1024);
            entity.Property(e => e.unit_price).HasPrecision(18);

            entity.HasOne(d => d.category).WithMany(p => p.products)
                .HasForeignKey(d => d.category_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("products_category_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
