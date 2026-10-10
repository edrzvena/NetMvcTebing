using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore; // library ef core, orm nya .NET
using Tebing.Data;
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

// Monitoring Grafana
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();