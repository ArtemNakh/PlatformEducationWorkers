using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Services;
using PlatformEducationWorkers.Storage;
using PlatformEducationWorkers.Storage.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PlatformEducationContex>(options => options.UseSqlServer("Local"));

//builder.Services.AddTransient<IAdministratorService,AdministratorService>();
builder.Services.AddTransient<IUserService,UserService>();
builder.Services.AddTransient<IUserResultService,UserResultService>();
builder.Services.AddTransient<IEnterpriceService,EnterpriceService>();
builder.Services.AddTransient<IRoleService,JobTitleService>();
builder.Services.AddTransient<ICourcesService,CourceService>();
builder.Services.AddTransient<IRepository,GenericRepository>();

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
