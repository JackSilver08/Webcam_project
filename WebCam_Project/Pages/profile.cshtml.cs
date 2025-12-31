using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebCam_Project.DBContext;
using WebCam_Project.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace WebCam_Project.Pages
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            AppDbContext db,
            IWebHostEnvironment env,
            ILogger<ProfileModel> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public User? UserInfo { get; set; }
        public List<PackagingRecord> Records { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            {
                return RedirectToPage("/Login");
            }

            UserInfo = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            Records = await _db.PackagingRecords
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostDeleteSingleAsync(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out long userId))
            {
                _logger.LogError("❌ Không parse được userId");
                return RedirectToPage();
            }

            _logger.LogInformation($"🟢 UserId = {userId}");

            await using var tx = await _db.Database.BeginTransactionAsync();
            _logger.LogInformation("🟢 Transaction started");

            // SET LOCAL
            await _db.Database.ExecuteSqlRawAsync(
       "SELECT set_config('app.user_id', @uid, true)",
       new Npgsql.NpgsqlParameter("uid", userId.ToString())
   );
            _logger.LogInformation("🟢 SET LOCAL app.user_id executed");

            var dbUserId = await _db.Database
    .SqlQueryRaw<long?>(
        "SELECT current_setting('app.user_id', true)::bigint AS \"Value\""
    )
    .FirstOrDefaultAsync();

            _logger.LogInformation($"🔍 DB current app.user_id = {dbUserId}");

            var record = await _db.PackagingRecords
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
            {
                _logger.LogWarning("❌ Record NULL (RLS chặn SELECT hoặc không tồn tại)");
                await tx.RollbackAsync();
                return RedirectToPage();
            }

            _logger.LogInformation(
                $"🟢 Found record Id={record.Id}, UserId={record.UserId}"
            );

            _db.PackagingRecords.Remove(record);
            _logger.LogInformation("🟡 Record marked for DELETE");

            var result = await _db.SaveChangesAsync();
            _logger.LogInformation($"🟢 SaveChanges result = {result}");

            await tx.CommitAsync();
            _logger.LogInformation("🟢 Transaction committed");

            return RedirectToPage();
        }


    }
}