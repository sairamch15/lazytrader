using AlgoTradingKiteApi.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<KiteService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<WebSocketService>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Set session timeout duration
    options.Cookie.HttpOnly = true; // Make session cookie inaccessible to JavaScript
    options.Cookie.IsEssential = true; // Ensure session cookie is sent even if user hasn't consented to other cookies
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<MarketDataHub>("/marketDataHub");

app.Run();
