using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurePath.Markets.Models;
using SurePath.Markets.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // needed for Identity UI scaffolding

// SQLite + EF Core
builder.Services.AddDbContext<MarketsDbContext>(options =>
    options.UseSqlite("Data Source=surepath_markets.db"));

// ASP.NET Core Identity with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<MarketsDbContext>();

// Point cookie middleware at our custom auth controller
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/auth/login";
    options.LogoutPath       = "/auth/logout";
    options.AccessDeniedPath = "/auth/login";
});

// ── Pipeline ──────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Auto-create DB on startup, then seed roles + users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketsDbContext>();
    db.Database.EnsureCreated();
    await SeedService.SeedAsync(scope.ServiceProvider);
}

await app.RunAsync();
