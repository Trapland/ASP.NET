using ASP_201MVC.Data;
using ASP_201MVC.Middleware;
using ASP_201MVC.Services;
using ASP_201MVC.Services.Email;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.Kdf;
using ASP_201MVC.Services.Random;
using ASP_201MVC.Services.RandomImg;
using ASP_201MVC.Services.Validation;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DateService>();
builder.Services.AddScoped<TimeService>();
builder.Services.AddSingleton<StampService>();

//bind IHashService to Md5HashService
builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IRandomService, RandomServiceV1>();
builder.Services.AddSingleton<IKdfService, HashKdfService>();
builder.Services.AddSingleton<IRandomImgName, RandomImgName>();
builder.Services.AddSingleton<IValidationService, ValidationServiceV1>();
builder.Services.AddSingleton<IEmailService, GmailService>();

//реєстрація контексту з підключення до MS SQL Server
//builder.Services.AddDbContext<DataContext>(options => 
//    options.UseSqlServer(
//    builder.Configuration.GetConnectionString("MsDb")));

//реєстрація контексту з підключення до MS SQL Server
//особливість - для контексту слід зазначити версію MySQL
//// варіант 1 - визначити версію та ввести дані
//ServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 23));
//builder.Services.AddDbContext<DataContext>(options => 
//    options.UseMySql(
//    builder.Configuration.GetConnectionString("MySqlDb"), 
//    serverVersion));

// варіант 2 - автоматичне визначення версії, але для цього треба
// попередньо створити підключення
String? connectionString = builder.Configuration.GetConnectionString("MySqlDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Add services to the container.
builder.Services.AddControllersWithViews();

//Налагодження сесій
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

app.UseRouting();

//включення механізму сесій
app.UseSession();

app.UseAuthorization();

app.UseSessionAuth();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
