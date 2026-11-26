using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using FINALS.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register");
});

builder.Services.AddHttpClient();   
builder.Services.AddTransient<FINALS.Services.EmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(context);

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    string adminPassword = builder.Configuration["Gaso:Password"]
        ?? throw new InvalidOperationException("Gaso:Password is not configured.");

    var admin = await userManager.FindByEmailAsync("gaso@antipolo.benilde.edu.ph");

    if (admin == null)
    {
        admin = new IdentityUser();
        admin.UserName = "gaso@antipolo.benilde.edu.ph";
        admin.Email = "gaso@antipolo.benilde.edu.ph";
        admin.EmailConfirmed = true;

        await userManager.CreateAsync(admin, adminPassword);
    }
    else
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(admin);
        await userManager.ResetPasswordAsync(admin, token, adminPassword);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();