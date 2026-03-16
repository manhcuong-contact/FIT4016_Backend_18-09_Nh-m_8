using CraftOutsourcing.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // <-- Thêm Authentication TRƯỚC Authorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

// --- TỰ ĐỘNG CẬP NHẬT LẠI HASH MẬT KHẨU ADMIN NẾU SAI ---
using (var scope = app.Services.CreateScope())
{
    var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var admin = _db.Users.FirstOrDefault(u => u.Username == "admin");
    if (admin != null)
    {
        // Gán cứng Hash chính xác của "admin123"
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        _db.SaveChanges();
    }
}

app.Run();
