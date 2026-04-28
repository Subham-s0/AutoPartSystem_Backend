using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
<<<<<<< HEAD

=======
using Microsoft.OpenApi;
using System.Security.Claims;
>>>>>>> main
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Seeders;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;
using VehiStock.Infrastructure.Repositories;
using VehiStock.Infrastructure.Settings;
using VehiStock.Infrastructure.Services;
using VehiStock.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);



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


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("SeedAdmin"));
builder.Services.Configure<AlertProcessingSettings>(builder.Configuration.GetSection("Alerts"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));



var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();


// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ✅ DEPENDENCY INJECTION
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
builder.Services.AddScoped<ICustomerPortalRepository, CustomerPortalRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
<<<<<<< HEAD

=======
builder.Services.AddScoped<IStaffManagementRepository, StaffManagementRepository>();
builder.Services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
builder.Services.AddScoped<IStaffReportRepository, StaffReportRepository>();
>>>>>>> main
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<ICustomerPortalService, CustomerPortalService>();
builder.Services.AddScoped<IAlertService, AlertService>();
<<<<<<< HEAD
builder.Services.AddScoped<IReportService, ReportService>();

// 🔥 EMAIL FEATURE (Feature 11)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<InvoiceTemplateService>();

// 🔥 BACKGROUND ALERT (Feature 15 ready)
builder.Services.AddHostedService<AlertBackgroundService>();


// ✅ CONTROLLERS + SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

=======
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
builder.Services.AddScoped<IStaffReportService, StaffReportService>();
builder.Services.AddHostedService<AlertBackgroundService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<InvoiceTemplateService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VehiStock API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT bearer token."
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
>>>>>>> main

var app = builder.Build();


// ✅ MIDDLEWARE
if (app.Environment.IsDevelopment())
{
<<<<<<< HEAD
    app.UseSwagger();
    app.UseSwaggerUI();
=======
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "VehiStock API v1");
        options.RoutePrefix = "swagger";
    });
>>>>>>> main
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var seedSettings = scope.ServiceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<AdminSeedSettings>>()
        .Value;

    await RoleSeeder.SeedAsync(roleManager);
    await AdminSeeder.SeedAsync(roleManager, userManager, seedSettings);
}

app.Run();