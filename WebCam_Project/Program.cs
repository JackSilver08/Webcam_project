using Microsoft.EntityFrameworkCore;
using WebCam_Project.DBContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";   // dùng popup login
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ================= APP =================
var app = builder.Build();

// ===== STATIC FILES (wwwroot) =====
app.UseStaticFiles();

// ===== STATIC FILES: Uploads (ngoài wwwroot) =====
var uploadsPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads"
});

// ===== PIPELINE ORDER (RẤT QUAN TRỌNG) =====
app.UseRouting();

app.UseAuthentication();   // ❗ BẮT BUỘC phải trước Authorization
app.UseAuthorization();

app.MapRazorPages();

app.Run();
