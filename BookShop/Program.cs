
using BookShop.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BookShop.Interfaces;
using BookShop.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<myShopContext>(options =>
    options.UseSqlServer(connectionString, options => options.CommandTimeout(3600))
    );

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<myShopContext>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });
builder.Services.AddAuthorization();
builder.Services.AddTransient<IUserInitializeService, AdminInitializeService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>(); // Added FileStorageService registration
//builder.Services.AddScoped<IUserInitializeService, AdminInitializeService>();
//builder.Services.AddScoped<myShopContext>();
//builder.WebHost.UseDefaultServiceProvider(options => options.ValidateScopes = false);//My try to solve the error

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

// Add memory cache support
builder.Services.AddMemoryCache();


//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//        {
//            policy.WithOrigins("*")
//                   .AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowedToAllowWildcardSubdomains();

//        });
//});
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "admin_area",
    pattern: "{area:exists=Admin}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "seller_area",
    pattern: "{area:exists=Seller}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}");

app.MapRazorPages();

//var cookiePolicyOptions = new CookiePolicyOptions
//{
//    MinimumSameSitePolicy = SameSiteMode.Strict
//};
//app.UseCookiePolicy(cookiePolicyOptions);

//Run service(create admin user) before program run
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var userInitializer = services.GetRequiredService<IUserInitializeService>();
    userInitializer.Initialize();
}

app.Run();