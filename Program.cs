using FormReporting.Data;
using FormReporting.Data.Seeders;
using FormReporting.Models.Entities.Identity;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure SQL Server with query splitting to avoid cartesian explosion
    // This significantly improves performance when using multiple .Include() statements
    // See: https://docs.microsoft.com/en-us/ef/core/querying/single-split-queries
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    );
});

// Configure Cookie Authentication (not using ASP.NET Core Identity to preserve existing User model)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add password hasher for secure password storage
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Add data protection for token generation (password reset tokens)
builder.Services.AddDataProtection();

// Register application services
// Forms services
builder.Services.AddScoped<IFormCategoryService, FormCategoryService>();
builder.Services.AddScoped<IFormTemplateService, FormTemplateService>();
builder.Services.AddScoped<IFormBuilderService, FormBuilderService>();
builder.Services.AddScoped<IFormItemOptionTemplateService, FormItemOptionTemplateService>();
builder.Services.AddScoped<IFormSubmissionService, FormSubmissionService>();
builder.Services.AddScoped<IFormResponseService, FormResponseService>();

// Metrics services
builder.Services.AddScoped<FormReporting.Services.Metrics.IMetricDefinitionService, FormReporting.Services.Metrics.MetricDefinitionService>();
builder.Services.AddScoped<FormReporting.Services.Metrics.IMetricMappingService, FormReporting.Services.Metrics.MetricMappingService>();
builder.Services.AddScoped<FormReporting.Services.Metrics.IMetricPopulationService, FormReporting.Services.Metrics.MetricPopulationService>();

// Identity services
builder.Services.AddScoped<IScopeService, ScopeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Organizational services
builder.Services.AddScoped<FormReporting.Services.Organizational.ITenantService, FormReporting.Services.Organizational.TenantService>();
builder.Services.AddScoped<FormReporting.Services.Organizational.IDepartmentService, FormReporting.Services.Organizational.DepartmentService>();

// Register authentication services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();
// TODO: Register remaining services (Step 6)
// builder.Services.AddScoped<INotificationService, NotificationService>();

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
        // ScopeLevelSeeder.SeedScopeLevels(context);
        
        // 2. Seed roles (depends on scope levels)
        // RoleSeeder.SeedRoles(context);
        
        // 3. Seed organizational structure
        // RegionSeeder.SeedRegions(context);
        // TenantSeeder.SeedTenants(context);
        
        // 4. Seed users (depends on roles, regions, and tenants)
       // UserSeeder.SeedUsers(context);
        
        // 5. Seed menu system
        // MenuSectionSeeder.SeedMenuSections(context);
        // ModuleSeeder.SeedModules(context);
        // MenuItemSeeder.SeedMenuItems(context);
        
        // 6. Seed Form Builder data
        // FormItemOptionTemplateSeeder.SeedOptionTemplates(context);

        // 7. Seed Metric Definitions (no dependencies)
       // MetricDefinitionSeeder.SeedMetricDefinitions(context);
        
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

// Authentication & Authorization middleware (must be in this order)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Map controllers (includes attribute-routed controllers like AccountController)
app.MapControllers();

// Conventional routing for other controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Root URL redirect to login
app.MapGet("/", () => Results.Redirect("/Account/Login"));


app.Run();
