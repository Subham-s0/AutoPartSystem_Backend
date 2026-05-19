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
    public DbSet<ServiceInvoice> ServiceInvoices => Set<ServiceInvoice>();
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

        builder.Entity<CustomerProfile>()
            .HasIndex(x => x.UserId)
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

        builder.Entity<PurchaseInvoice>()
            .HasIndex(x => x.InvoiceNo)
            .IsUnique();

        builder.Entity<SalesInvoice>()
            .HasIndex(x => x.InvoiceNo)
            .IsUnique();

        builder.Entity<ServiceInvoice>()
            .HasIndex(x => x.ServiceRecordId)
            .IsUnique();

        builder.Entity<Review>()
            .HasIndex(x => new { x.CustomerId, x.ServiceRecordId })
            .IsUnique();

        builder.Entity<CustomerProfile>()
            .Property(x => x.RegistrationSource)
            .HasConversion<string>();

        builder.Entity<Notification>()
            .Property(x => x.NotificationType)
            .HasConversion<string>();

        builder.Entity<Notification>()
            .Property(x => x.ReferenceType)
            .HasMaxLength(50);

        builder.Entity<Notification>()
            .HasIndex(x => new { x.UserId, x.NotificationType, x.ReferenceType, x.ReferenceId, x.CreatedAt });

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

        builder.Entity<ServiceInvoice>()
            .Property(x => x.PaymentStatus)
            .HasConversion<string>();

        builder.Entity<Payment>()
            .Property(x => x.PaymentType)
            .HasConversion<string>();

        builder.Entity<Payment>()
            .ToTable("Payments", table => table.HasCheckConstraint(
                "CK_Payments_ExactlyOneInvoice",
                "(\"SalesInvoiceId\" IS NOT NULL AND \"ServiceInvoiceId\" IS NULL) OR (\"SalesInvoiceId\" IS NULL AND \"ServiceInvoiceId\" IS NOT NULL)"));

        builder.Entity<Payment>()
            .HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SalesInvoiceId);

        builder.Entity<Payment>()
            .HasOne(x => x.ServiceInvoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.ServiceInvoiceId);

        builder.Entity<Appointment>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<ServiceRecord>()
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(ServiceRecordStatus.Open)
            .HasSentinel(ServiceRecordStatus.Open);

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

        builder.Entity<ServiceInvoice>()
            .Property(x => x.LaborCharge)
            .HasPrecision(18, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.PartsCharge)
            .HasPrecision(18, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.AmountPaid)
            .HasPrecision(18, 2);

        builder.Entity<ServiceInvoice>()
            .Property(x => x.BalanceDue)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}
