using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Seeders;
using VehiStock.Entities;
using VehiStock.Infrastructure.Configurations;
using VehiStock.Infrastructure.Persistance;
using VehiStock.Infrastructure.Repositories;
using VehiStock.Infrastructure.Services;
using VehiStock.Infrastructure.Settings;
using VehiStock.Application.Services;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

#region DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:5174",
        "http://127.0.0.1:5174",
        "http://localhost:5175",
        "http://127.0.0.1:5175",
        "http://localhost:4173",
        "http://127.0.0.1:4173"
    ];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("PostmanPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

#region IDENTITY
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
#endregion

#region CONFIG
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("SeedAdmin"));
builder.Services.Configure<AlertProcessingSettings>(builder.Configuration.GetSection("Alerts"));
builder.Services.Configure<ImageUploadSettings>(builder.Configuration.GetSection("ImageUpload"));
builder.Services.Configure<KhaltiSettings>(builder.Configuration.GetSection("Khalti"));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));
#endregion

#region JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new Exception("JWT missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt.SecretKey))
    };
});
#endregion

builder.Services.AddAuthorization();

#region CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowReactApp", p =>
        p.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
         .AllowAnyHeader()
         .AllowAnyMethod());
});
#endregion

#region SERVICES & REPOSITORIES
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
builder.Services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
builder.Services.AddScoped<ICustomerHistoryRepository, CustomerHistoryRepository>();
builder.Services.AddScoped<ICustomerServiceInvoiceRepository, CustomerServiceInvoiceRepository>();
builder.Services.AddScoped<IPaymentServiceRepository, PaymentServiceRepository>();
builder.Services.AddScoped<ICustomerPartRequestRepository, CustomerPartRequestRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IStaffManagementRepository, StaffManagementRepository>();
builder.Services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
builder.Services.AddScoped<IStaffReportRepository, StaffReportRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IAdminPartRequestRepository, AdminPartRequestRepository>();

// Parts & Purchase Invoices
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
builder.Services.AddScoped<ICustomerHistoryService, CustomerHistoryService>();
builder.Services.AddScoped<ICustomerServiceInvoiceService, CustomerServiceInvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICustomerPartRequestService, CustomerPartRequestService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
builder.Services.AddScoped<IStaffReportService, StaffReportService>();
builder.Services.AddScoped<IVendorService, VendorService>();

// Parts, Purchase Invoices, Analytics, & Dashboard
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IStaffDashboardService, StaffDashboardService>();
builder.Services.AddScoped<IStaffAppointmentService, StaffAppointmentService>();
builder.Services.AddScoped<IAdminPartRequestService, AdminPartRequestService>();

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<InvoiceTemplateService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IServiceInvoicePaymentService, ServiceInvoicePaymentService>();
builder.Services.AddScoped<ISalesInvoicePaymentService, SalesInvoicePaymentService>();
builder.Services.AddHttpClient<IKhaltiClient, KhaltiClient>();
#endregion

builder.Services.AddHostedService<AlertBackgroundService>();

#region CONTROLLERS
builder.Services.AddControllers()
.AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
#endregion

#region SWAGGER
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VehiStock API",
        Version = "v1"
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
#endregion

var app = builder.Build();

#region PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors(FrontendCorsPolicy);
app.UseCors("AllowReactApp");
app.UseCors("PostmanPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

#region SEED
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var seedSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AdminSeedSettings>>().Value;

    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration pre-check warning: {ex.Message}");
    }

    await RoleSeeder.SeedAsync(roleManager);
    await AdminSeeder.SeedAsync(roleManager, userManager, seedSettings);

    // Seed/Ensure Staff accounts exist with requested password Staff@1234
    var staffEmails = new[] { "staff@vehistock.com", "satff@vehistock.com" };
    foreach (var email in staffEmails)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                FullName = email == "satff@vehistock.com" ? "Seeded Satff Member" : "Default Staff",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true
            };
            var createRes = await userManager.CreateAsync(user, "Staff@1234");
            if (!createRes.Succeeded)
            {
                var errors = string.Join(", ", createRes.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed staff user '{email}': {errors}");
            }
            
            await userManager.AddToRoleAsync(user, VehiStock.Domain.Constants.RoleNames.Staff);
        }
        else
        {
            var isPassValid = await userManager.CheckPasswordAsync(user, "Staff@1234");
            if (!isPassValid)
            {
                var removeRes = await userManager.RemovePasswordAsync(user);
                if (removeRes.Succeeded)
                {
                    var addRes = await userManager.AddPasswordAsync(user, "Staff@1234");
                    if (!addRes.Succeeded)
                    {
                        var errors = string.Join(", ", addRes.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to set password for '{email}': {errors}");
                    }
                }
            }

            if (!await userManager.IsInRoleAsync(user, VehiStock.Domain.Constants.RoleNames.Staff))
            {
                await userManager.AddToRoleAsync(user, VehiStock.Domain.Constants.RoleNames.Staff);
            }
        }

        // Programmatically ensure the StaffProfile record exists for foreign key linkages
        if (user != null && !string.IsNullOrEmpty(user.Id))
        {
            var hasProfile = await dbContext.StaffProfiles.AnyAsync(p => p.UserId == user.Id);
            if (!hasProfile)
            {
                var staffProfile = new VehiStock.Entities.StaffProfile
                {
                    UserId = user.Id,
                    JobTitle = "Default Staff",
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                dbContext.StaffProfiles.Add(staffProfile);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
#endregion

app.Run();