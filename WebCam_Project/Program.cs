using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using System.Security.Claims;
using WebCam_Project.DBContext;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";           // sửa thành trang login thật
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ================= APP =================
var app = builder.Build();

// Static files
app.UseStaticFiles();

// Uploads folder
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads"
});

// Pipeline
app.UseRouting();

app.UseAuthentication();

// ✅ Middleware set app.user_id cho RLS - PHIÊN BẢN ĐÚNG
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out long uid))
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                "SELECT set_config('app.user_id', @uid, true)",
                new NpgsqlParameter("uid", uid.ToString())
            );
        }
    }

    await next();
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();