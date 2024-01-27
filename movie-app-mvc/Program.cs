using Microsoft.AspNetCore.Authentication.Cookies;
using movie_app_mvc.Controllers;
using MovieApp.Data.Context;
using MovieApp.Services.Interfaces;
using MovieApp.Services;
using MovieApp.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Index"; // Replace with your login page URL
    options.AccessDeniedPath = "/Home/Error"; // Replace with your error page URL
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MovieDbContext>();
builder.Services.AddScoped<IMovieDbContext, MovieDbContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDetailsController, DetailsController>();
builder.Services.AddScoped<ILoginController, LoginController>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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

app.UseRouting();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
