using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCam_Project.DBContext;
using WebCam_Project.Models;

namespace WebCam_Project.Pages
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _db;

        public ProfileModel(AppDbContext db)
        {
            _db = db;
        }

        public User UserInfo { get; set; } = null!;
        public List<PackagingRecord> Records { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToPage("/Index");

            long userId = long.Parse(userIdClaim.Value);

            UserInfo = await _db.Users
                .FirstAsync(x => x.Id == userId);

            Records = await _db.PackagingRecords
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Index");
        }
    }
}
