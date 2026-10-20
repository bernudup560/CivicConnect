using CivicConnect.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Register cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "Cookies";
    });

// Use an in-memory ticket API client for local development and testing (singleton so state persists)
builder.Services.AddSingleton<ITicketApiClient, TicketApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsStaging())
{
    app.UseHttpsRedirection();
}
app.UseHttpsRedirection();


app.UseStaticFiles();
// (Correlation middleware and lightweight telemetry removed)
app.UseRouting();
// Enable authentication middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();