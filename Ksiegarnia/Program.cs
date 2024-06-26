using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<KsiegarniaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DeafultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<KsiegarniaDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CustomerBooks}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
