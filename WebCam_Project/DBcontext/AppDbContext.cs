using Microsoft.EntityFrameworkCore;
using WebCam_Project.Models;

namespace WebCam_Project.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /* ===== DbSet ===== */
        public DbSet<User> Users { get; set; }
        public DbSet<PackagingRecord> PackagingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* ================= USER ================= */
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("id");

                entity.Property(e => e.Username)
                      .HasColumnName("username")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => e.Username)
                      .IsUnique();

                entity.Property(e => e.PasswordHash)
                      .HasColumnName("password_hash")
                      .IsRequired();

                entity.Property(e => e.FullName)
                      .HasColumnName("full_name")
                      .HasMaxLength(100);

                entity.Property(e => e.Email)
                      .HasColumnName("email")
                      .HasMaxLength(100);

                entity.Property(e => e.Role)
                      .HasColumnName("role")
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.IsActive)
                      .HasColumnName("is_active");

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("updated_at");
            });

            /* ================= PACKAGING RECORD ================= */
            modelBuilder.Entity<PackagingRecord>(entity =>
            {
                entity.ToTable("packaging_records", "public");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("id");

                entity.Property(e => e.ProductCode)
                      .HasColumnName("product_code")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.VideoPath)
                      .HasColumnName("video_path")
                      .IsRequired();

                entity.Property(e => e.RecordStart)
                      .HasColumnName("record_start");

                entity.Property(e => e.RecordEnd)
                      .HasColumnName("record_end");

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasMaxLength(20);

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at");
                entity.Property(e => e.ProductName)
                     .HasColumnName("product_name")
                     .HasMaxLength(50)
                     .IsRequired(false);


                /* ===== FK USER ===== */
                entity.Property(e => e.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.PackagingRecords)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
