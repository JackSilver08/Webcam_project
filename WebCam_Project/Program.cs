using Microsoft.EntityFrameworkCore;
using WebCam_Project.DBContext;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
