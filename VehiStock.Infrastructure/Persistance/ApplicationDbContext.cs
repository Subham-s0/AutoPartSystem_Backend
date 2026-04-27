using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Persistance;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PartCategory> PartCategories => Set<PartCategory>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<ServiceRecordPart> ServiceRecordParts => Set<ServiceRecordPart>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity keeps these join tables internally; declare the composite keys explicitly.
        builder.Entity<IdentityUserRole<string>>()
            .HasKey(x => new { x.UserId, x.RoleId });

        builder.Entity<IdentityUserLogin<string>>()
            .HasKey(x => new { x.LoginProvider, x.ProviderKey });

        builder.Entity<IdentityUserToken<string>>()
            .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        // Enforce the project rule that one user can hold only one role.
        builder.Entity<IdentityUserRole<string>>()
            .HasIndex(x => x.UserId) 
            .IsUnique();

        builder.Entity<ApplicationUser>()
            .Property(x => x.FullName)
            .HasMaxLength(150);

        builder.Entity<RefreshToken>()
            .HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.Entity<StaffProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<StaffProfile>()
            .HasIndex(x => x.StaffCode)
            .IsUnique();

        builder.Entity<CustomerProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<CustomerProfile>()
            .HasIndex(x => x.CustomerCode)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasIndex(x => x.VehicleNumber)
            .IsUnique();

        builder.Entity<Vendor>()
            .HasIndex(x => x.VendorCode)
            .IsUnique();

        builder.Entity<PartCategory>()
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.Entity<Part>()
            .HasIndex(x => x.PartCode)
            .IsUnique();

        builder.Entity<PurchaseInvoice>()
            .HasIndex(x => x.InvoiceNo)
            .IsUnique();

        builder.Entity<SalesInvoice>()
            .HasIndex(x => x.InvoiceNo)
            .IsUnique();

        builder.Entity<CustomerProfile>()
            .Property(x => x.RegistrationSource)
            .HasConversion<string>();

        builder.Entity<Notification>()
            .Property(x => x.NotificationType)
            .HasConversion<string>();

        builder.Entity<PartRequest>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<PurchaseInvoice>()
            .Property(x => x.PaymentStatus)
            .HasConversion<string>();

        builder.Entity<SalesInvoice>()
            .Property(x => x.PaymentType)
            .HasConversion<string>();

        builder.Entity<SalesInvoice>()
            .Property(x => x.PaymentStatus)
            .HasConversion<string>();

        builder.Entity<Payment>()
            .Property(x => x.PaymentType)
            .HasConversion<string>();

        builder.Entity<Appointment>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<Part>()
            .Property(x => x.UnitCost)
            .HasPrecision(18, 2);

        builder.Entity<Part>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoice>()
            .Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoice>()
            .Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoice>()
            .Property(x => x.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoice>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoiceItem>()
            .Property(x => x.UnitCost)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseInvoiceItem>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.AmountPaid)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoice>()
            .Property(x => x.BalanceDue)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoiceItem>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoiceItem>()
            .Property(x => x.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<SalesInvoiceItem>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        builder.Entity<ServiceRecord>()
            .Property(x => x.LaborCharge)
            .HasPrecision(18, 2);

        builder.Entity<ServiceRecord>()
            .Property(x => x.PartsCharge)
            .HasPrecision(18, 2);

        builder.Entity<ServiceRecord>()
            .Property(x => x.TotalCharge)
            .HasPrecision(18, 2);

        builder.Entity<ServiceRecordPart>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<ServiceRecordPart>()
            .Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}
