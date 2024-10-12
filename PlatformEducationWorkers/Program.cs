using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PlatformEducationContex>(options => options.UseSqlServer("Local"));

builder.Services.AddTransient<IAdministratorService,AdministratorService>();
builder.Services.AddTransient<IUserService,UserService>();
builder.Services.AddTransient<IUserResultService,UserResultService>();
builder.Services.AddTransient<IEnterpriceService,EnterpriceService>();
builder.Services.AddTransient<IRoleService,RoleService>();
builder.Services.AddTransient<ICourcesService,CourceService>();

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
