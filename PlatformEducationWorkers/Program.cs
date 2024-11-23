using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Storage;
using PlatformEducationWorkers.Storage.Repositories;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PlatformEducationContex>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("Local")));


// Додавання сесії та отримання тайм-ауту з конфігураційного файлу
//var sessionTimeout = builder.Configuration.GetValue<int>("Session:IdleTimeout");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeout")); // Використання значення з appsettings.json
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//builder.Services.AddTransient<IAdministratorService,AdministratorService>();
builder.Services.AddTransient<IUserService,UserService>();
builder.Services.AddTransient<IUserResultService,UserResultService>();
builder.Services.AddTransient<IEnterpriseService,EnterpriceService>();
builder.Services.AddTransient<IJobTitleService,JobTitleService>();
builder.Services.AddTransient<ICourcesService,CourceService>();
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

// Додаємо перенаправлення на стартову сторінку(Login)
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
