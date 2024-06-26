using Ksiegarnia.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<KsiegarniaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DeafultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CustomerBooks}/{action=Index}/{id?}");


app.Run();
