using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore; // library ef core, orm nya .NET
using Microsoft.Extensions.FileProviders;
using Tebing.Data;
using Tebing.Data.Seed;
using Tebing.Repositories.Implementations;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Implementations;
using Tebing.Services.Interfaces;

// Monitoring Grafana
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Monitoring Grafana
builder.Host.UseSerilog((context, config) => config
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        "http://localhost:3100",
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = "tebing" }
        }));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMountainRepository, MountainRepository>();
builder.Services.AddScoped<IHikingRouteRepository, HikingRouteRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMountainService, MountainService>();
builder.Services.AddScoped<IMountainImageService, MountainImageService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Data contoh (akun admin/pendaki + gunung) — cuma di Development, lihat Data/Seed/DbSeeder.cs.
if (app.Environment.IsDevelopment())
    await DbSeeder.SeedAsync(app.Services);

// Monitoring Grafana
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 404 (NotFound()) dkk dirender pakai halaman Tebing, bukan halaman kosong browser.
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

app.UseHttpsRedirection();

// Foto gunung upload admin ditambah waktu app jalan, jadi gak ada di manifest build MapStaticAssets —
// disajikan terpisah. Nama file-nya unik & gak pernah ditimpa, jadi boleh di-cache browser selamanya.
var mountainUploadFolder = Path.Combine(app.Environment.WebRootPath, "uploads", "mountains");
Directory.CreateDirectory(mountainUploadFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mountainUploadFolder),
    RequestPath = MountainImageService.UploadRequestPath,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();