using ASP_201MVC.Data;
using ASP_201MVC.Services;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.Kdf;
using ASP_201MVC.Services.Random;
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

//��������� ��������� � ���������� �� MS SQL Server
//builder.Services.AddDbContext<DataContext>(options => 
//    options.UseSqlServer(
//    builder.Configuration.GetConnectionString("MsDb")));

//��������� ��������� � ���������� �� MS SQL Server
//���������� - ��� ��������� ��� ��������� ����� MySQL
//// ������ 1 - ��������� ����� �� ������ ���
//ServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 23));
//builder.Services.AddDbContext<DataContext>(options => 
//    options.UseMySql(
//    builder.Configuration.GetConnectionString("MySqlDb"), 
//    serverVersion));

// ������ 2 - ����������� ���������� ����, ��� ��� ����� �����
// ���������� �������� ����������
String? connectionString = builder.Configuration.GetConnectionString("MySqlDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Add services to the container.
builder.Services.AddControllersWithViews();

//������������ ����
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
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

//��������� �������� ����
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
