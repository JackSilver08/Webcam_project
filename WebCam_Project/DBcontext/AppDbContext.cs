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

        public DbSet<PackagingRecord> PackagingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PackagingRecord>(entity =>
            {
                entity.ToTable("packaging_records");

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
            });
        }
    }
}
