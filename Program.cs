using FormReporting.Data;
using FormReporting.Data.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // IMPORTANT: Seed in order of dependencies
        
        // 1. Seed scope levels (required before roles)
        ScopeLevelSeeder.SeedScopeLevels(context);
        
        // 2. Seed roles (depends on scope levels)
        RoleSeeder.SeedRoles(context);
        
        // 3. Seed organizational structure
        // RegionSeeder.SeedRegions(context);
        // TenantSeeder.SeedTenants(context);
        
        // 4. Seed menu system
        // MenuSectionSeeder.SeedMenuSections(context);
        // ModuleSeeder.SeedModules(context);
        // MenuItemSeeder.SeedMenuItems(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
