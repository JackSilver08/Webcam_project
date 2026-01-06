using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCam_Project.DBContext;
using WebCam_Project.Models;

namespace WebCam_Project.Pages
{
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _db;

        public AdminModel(AppDbContext db)
        {
            _db = db;
        }

        /* ===== DATA ===== */
        public List<User> Users { get; set; } = new();
        public List<PackagingRecord> Records { get; set; } = new();

        public bool SelectedUser => UserId.HasValue;

        /* ===== FILTER PARAMS ===== */
        [BindProperty(SupportsGet = true)]
        public long? UserId { get; set; }

        // Keyword tìm cả mã và tên sản phẩm
        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }
        // Helper: lấy timezone VN (Windows / Linux)
        static TimeZoneInfo GetVnTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        }
        public void OnGet()
        {
            Users = _db.Users.OrderBy(u => u.Username).ToList();
            if (!UserId.HasValue) return;

            var query = _db.PackagingRecords.Where(r => r.UserId == UserId.Value);

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                var kw = Keyword.Trim();
                query = query.Where(r =>
                    r.ProductCode.Contains(kw) ||
                    (r.ProductName != null && r.ProductName.Contains(kw)));
            }

            var vnTz = GetVnTimeZone();

            // FromDate: từ 00:00 giờ VN -> UTC
            if (FromDate.HasValue)
            {
                var startLocal = DateTime.SpecifyKind(FromDate.Value.Date, DateTimeKind.Unspecified);
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, vnTz);
                query = query.Where(r => r.CreatedAt >= startUtc);
            }

            // ToDate: đến hết ngày (exclusive ngày kế tiếp) theo giờ VN -> UTC
            if (ToDate.HasValue)
            {
                var endExclusiveLocal = DateTime.SpecifyKind(ToDate.Value.Date.AddDays(1), DateTimeKind.Unspecified);
                var endExclusiveUtc = TimeZoneInfo.ConvertTimeToUtc(endExclusiveLocal, vnTz);
                query = query.Where(r => r.CreatedAt < endExclusiveUtc);
            }

            Records = query.OrderByDescending(r => r.CreatedAt).ToList();
        }
    }
}
