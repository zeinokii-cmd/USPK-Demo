using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using USPK_Project.IServices;
using USPK_Project.Models;
using USPK_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// Defer constructing the service until a database adapter has been registered.
builder.Services.AddScoped<IUserService>(services => new UserService(
    services.GetRequiredService<IUserStore>(), services, services.GetRequiredService<IPasswordHasher<User>>()));
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthorizationHandler, AdminAuthorizationHandler>();

var demoEnabled = builder.Configuration.GetValue<bool>("Demo:Enabled");
if (demoEnabled)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Demo mode is only available in the Development environment.");
    var rawConnectionString = builder.Configuration.GetConnectionString("Demo")
        ?? throw new InvalidOperationException("ConnectionStrings:Demo is required for demo mode.");
    var connection = new NpgsqlConnectionStringBuilder(rawConnectionString);
    if (connection.Host is not ("127.0.0.1" or "localhost") || connection.Port != 55433 ||
        connection.Database != "uspk_demo" || connection.Username != "uspk_demo")
        throw new InvalidOperationException("Demo mode only accepts the dedicated local uspk_demo database on port 55433.");
    if (!JwtService.IsConfigured(builder.Configuration) || builder.Configuration["Authorization:AdministratorRole"] != "admin")
        throw new InvalidOperationException("Demo mode requires JWT settings and the admin role.");

    builder.Services.AddSingleton(NpgsqlDataSource.Create(rawConnectionString));
    builder.Services.AddScoped<DemoPostgresStore>();
    builder.Services.AddScoped<IUserStore>(services => services.GetRequiredService<DemoPostgresStore>());
    builder.Services.AddScoped<IRoleCatalog>(services => services.GetRequiredService<DemoPostgresStore>());
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // An installation without JWT settings can start, but cannot issue or accept tokens.
        options.TokenValidationParameters = JwtService.IsConfigured(builder.Configuration)
            ? JwtService.CreateValidationParameters(builder.Configuration)
            : new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)),
                ValidateIssuer = true,
                ValidIssuer = "unconfigured",
                ValidateAudience = true,
                ValidAudience = "unconfigured",
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero
            };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("CreateUsers", policy => policy.RequireAuthenticatedUser()
        .AddRequirements(new AdminRoleRequirement())));

var app = builder.Build();
if (demoEnabled) await DemoDatabaseInitializer.InitializeAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!demoEnabled) app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
