using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCam_Project.DBContext;
using WebCam_Project.Models;

namespace WebCam_Project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            AppDbContext db,
            IWebHostEnvironment env,
            ILogger<IndexModel> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        /* ================= UPLOAD HANDLER ================= */
        public async Task<IActionResult> OnPostUploadAsync()
        {
            _logger.LogWarning("🚨 Upload handler HIT"); 
            try
            {
                var file = Request.Form.Files["video"];
                var productCode = Request.Form["product_code"].ToString();

                if (file == null || string.IsNullOrWhiteSpace(productCode))
                    return BadRequest("Missing video or product_code");

                // ❌ Check trùng sản phẩm
                bool exists = _db.PackagingRecords
                    .Any(x => x.ProductCode == productCode);

                if (exists)
                    return StatusCode(409, "Product already recorded");

                // 📁 uploads/
                var uploadDir = Path.Combine(_env.ContentRootPath, "Uploads");
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{productCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.webm";
                var filePath = Path.Combine(uploadDir, fileName);

                // 💾 Save file
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                // 🧾 Save DB
                var record = new PackagingRecord
                {
                    ProductCode = productCode,
                    VideoPath = $"Uploads/{fileName}",
                    RecordStart = DateTime.UtcNow,
                    RecordEnd = DateTime.UtcNow,
                    Status = "COMPLETED",
                    CreatedAt = DateTime.UtcNow
                };

                _db.PackagingRecords.Add(record);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Saved packaging record: {Code}", productCode);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
