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
using PlatformEducationWorkers.Storage;
using PlatformEducationWorkers.Storage.Repositories;

var builder = WebApplication.CreateBuilder(args);

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


builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.AddScoped<ILogRepository, LogsRepository>();
builder.Services.AddTransient<IUserService,UserService>();
builder.Services.AddTransient<IUserResultService,UserResultService>();
builder.Services.AddTransient<IEnterpriseService,EnterpriceService>();
builder.Services.AddTransient<IJobTitleService,JobTitleService>();
builder.Services.AddTransient<ICoursesService,CourseService>();
builder.Services.AddScoped<IRepository,GenericRepository>();
builder.Services.AddScoped<IEnterpriseRepository,EnterpriseRepository>();
builder.Services.AddTransient<ICreateEnterpriseService, CreateEnterpriseService>();


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

app.UseSession();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path == "/")
    {
        context.Response.Redirect("/Login/Login");
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
