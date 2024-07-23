using Microsoft.EntityFrameworkCore;
using RMTasks.Models;
using RMTasks.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("RingoTskDBcs") ?? throw new InvalidOperationException("Connection string 'RingoTskDBcs' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString)
.UseLazyLoadingProxies());

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddHostedService<NotifyReminderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.Configure<SMTPConfig>(builder.Configuration.GetSection("SMTPConfig"));
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
