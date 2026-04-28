using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Seeders;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;
using VehiStock.Infrastructure.Repositories;
using VehiStock.Infrastructure.Settings; 
using VehiStock.Infrastructure.Services;
using VehiStock.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<ApplicationRole>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("SeedAdmin"));
builder.Services.Configure<AlertProcessingSettings>(builder.Configuration.GetSection("Alerts"));

var jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are not configured.");
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey is not configured.");
}

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

authenticationBuilder.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
builder.Services.AddScoped<ICustomerPortalRepository, CustomerPortalRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<ICustomerPortalService, CustomerPortalService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddHostedService<AlertBackgroundService>();

// ✔ FIX 2 (EmailSettings binding works now)
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// ✔ FIX 3 (Email DI correct)
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<InvoiceTemplateService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var seedSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AdminSeedSettings>>().Value;

    await RoleSeeder.SeedAsync(roleManager);
    await AdminSeeder.SeedAsync(roleManager, userManager, seedSettings);
}

app.Run();