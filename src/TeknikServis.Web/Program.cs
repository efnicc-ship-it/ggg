using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using TeknikServis.Application;
using TeknikServis.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/teknikservis-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// MVC + Razor Views
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR
builder.Services.AddSignalR();

// Cookie auth (MVC)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/giris";
    options.LogoutPath = "/cikis";
    options.AccessDeniedPath = "/erisim-engellendi";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 5;
    });
    options.AddFixedWindowLimiter("public", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 20;
    });
});

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/hata");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hubs
app.MapHub<TeknikServis.Web.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<TeknikServis.Web.Hubs.ChatHub>("/hubs/chat");

// Hangfire Dashboard (admin only)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new TeknikServis.Web.Infrastructure.HangfireAuthFilter() }
});

// Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
