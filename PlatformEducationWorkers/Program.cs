using System;
using Amazon;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Core.Services.Enterprises;
using PlatformEducationWorkers.Logger;
using PlatformEducationWorkers.Models;
using PlatformEducationWorkers.Models.Azure;
using PlatformEducationWorkers.Storage;
using PlatformEducationWorkers.Storage.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Завантаження конфігурації
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Налаштування Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

Log.Information("Application starting up...");

// Реєстрація залежностей
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PlatformEducationContex>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("LocalDb")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeout"));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserResultService, UserResultService>();
builder.Services.AddTransient<IEnterpriseService, EnterpriceService>();
builder.Services.AddTransient<IJobTitleService, JobTitleService>();
builder.Services.AddTransient<ICoursesService, CourseService>();
builder.Services.AddScoped<IRepository, GenericRepository>();
builder.Services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();
builder.Services.AddTransient<ICreateEnterpriseService, CreateEnterpriseService>();
builder.Services.AddSingleton<AzureBlobCourseOperation>();
builder.Services.AddSingleton<AzureBlobAvatarOperation>();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path == "/")
    {
        context.Response.Redirect("/Login");
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");



app.Run();
