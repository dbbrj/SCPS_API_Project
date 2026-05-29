using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Data;
using SCPS_API_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<WeatherContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherContext")
        ?? throw new InvalidOperationException("Connection string 'WeatherContext' not found.")));

// Weather API facade — scoped HttpClient per request
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>();

// Background service (Singleton) that fetches snapshots on a timer
builder.Services.AddSingleton<WeatherFetchService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WeatherFetchService>());

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
