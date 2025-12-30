using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebCam_Project.DBContext;
using WebCam_Project.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public string? CurrentUsername { get; set; }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                CurrentUsername = User.Identity.Name;
            }
        }

        /* ================= LOGIN ================= */
        public async Task<IActionResult> OnPostLoginAsync(
            string username,
            string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == username && x.IsActive);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Sai tài khoản hoặc mật khẩu"
                });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return new JsonResult(new
            {
                success = true,
                username = user.Username
            });
        }

        /* ================= REGISTER ================= */
        public async Task<IActionResult> OnPostRegisterAsync(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Thiếu thông tin"
                });
            }

            bool exists = await _db.Users
                .AnyAsync(x => x.Username == username);

            if (exists)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Username đã tồn tại"
                });
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "USER",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

       

        /* ================= UPLOAD ================= */
        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (!User.Identity!.IsAuthenticated)
                return Unauthorized();

            try
            {
                var userId = long.Parse(
                    User.FindFirst("UserId")!.Value);

                var file = Request.Form.Files["video"];
                var productCode = Request.Form["product_code"].ToString();

                if (file == null || string.IsNullOrWhiteSpace(productCode))
                    return BadRequest();

                bool exists = _db.PackagingRecords
                    .Any(x => x.ProductCode == productCode);

                if (exists)
                    return StatusCode(409);

                var uploadDir = Path.Combine(_env.ContentRootPath, "Uploads");
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{productCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.webm";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var record = new PackagingRecord
                {
                    ProductCode = productCode,
                    VideoPath = $"Uploads/{fileName}",
                    RecordStart = DateTime.UtcNow,
                    RecordEnd = DateTime.UtcNow,
                    Status = "COMPLETED",
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _db.PackagingRecords.Add(record);
                await _db.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed");
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new JsonResult(new { success = true });
        }


    }
}
