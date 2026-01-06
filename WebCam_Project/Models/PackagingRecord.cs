using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCam_Project.Models
{
    [Table("packaging_records")]
    public class PackagingRecord
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("product_code")]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [Column("video_path")]
        public string VideoPath { get; set; } = string.Empty;

        [Column("record_start")]
        public DateTime RecordStart { get; set; }

        [Column("record_end")]
        public DateTime RecordEnd { get; set; }

        [MaxLength(20)]
        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [MaxLength(50)]
        [Column("product_name")]
        public string? ProductName { get; set; }

        /* ===== USER LINK ===== */

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
