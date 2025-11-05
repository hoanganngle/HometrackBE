using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace DataAccess
{
    // Chốt tên chung cho các version DB "HomeTrackDBContext"
    public class HomeTrackDBContext : DbContext
    {
        public HomeTrackDBContext(DbContextOptions<HomeTrackDBContext> options) : base(options) { }
        public HomeTrackDBContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("ServerConnection"));
            }
        }

        // DbSet hiện có
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<OTPEmail> OTPEmails { get; set; }

        // DbSet billing (ver 1,0 đang có 8 bảng, có thể remove Refunds nếu out of scope)
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanPrice> PlanPrices => Set<PlanPrice>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<OTPEmail>().ToTable("OTPEmail");

            // GUID mặc định (không đổi)
            modelBuilder.Entity<User>().Property(u => u.UserId).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<OTPEmail>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            // Quan hệ User-Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(255);

            // ===== Plans (gói)=====
            modelBuilder.Entity<Plan>(e =>
            {
                e.ToTable("Plans");
                e.HasKey(x => x.PlanId);
                e.Property(x => x.PlanId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Code).HasMaxLength(50).IsRequired();
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.IsActive).HasDefaultValue(true);
            });

            // ===== PlanPrices (giá gói cac thứ)=====
            modelBuilder.Entity<PlanPrice>(e =>
            {
                e.ToTable("PlanPrices");
                e.HasKey(x => x.PlanPriceId);
                e.Property(x => x.PlanPriceId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Period).HasConversion<int>();
                e.Property(x => x.DurationInDays).IsRequired();
                e.Property(x => x.AmountVnd).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasOne(x => x.Plan)
                 .WithMany(p => p.Prices)
                 .HasForeignKey(x => x.PlanId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.PlanId, x.Period, x.IsActive });
            });

            // ===== Subscriptions =====
            modelBuilder.Entity<Subscription>(e =>
            {
                e.ToTable("Subscriptions");
                e.HasKey(x => x.SubscriptionId);
                e.Property(x => x.SubscriptionId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.CancelAtPeriodEnd).HasDefaultValue(false);

                e.HasOne(x => x.Plan)
                 .WithMany()
                 .HasForeignKey(x => x.PlanId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK tới User (bạn có UserId trong entity)
                e.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.UserId, x.Status, x.CurrentPeriodEnd });

            });

            // ===== Orders =====
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.OrderId);
                e.Property(x => x.OrderId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.OrderCode).IsRequired();
                e.HasIndex(x => x.OrderCode).IsUnique();

                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.AmountVnd).IsRequired();
                e.Property(x => x.ReturnUrl).HasMaxLength(500);
                e.Property(x => x.CancelUrl).HasMaxLength(500);

                e.HasOne(x => x.Subscription).WithMany(s => s.Orders)
                    .HasForeignKey(x => x.SubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.PlanPrice).WithMany(p => p.Orders)
                    .HasForeignKey(x => x.PlanPriceId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.UserId, x.CreatedAt });
            });


            // ===== PaymentTransactions =====
            modelBuilder.Entity<PaymentTransaction>(e =>
            {
                e.ToTable("PaymentTransactions");
                e.HasKey(x => x.PaymentTransactionId);
                e.Property(x => x.PaymentTransactionId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Provider).HasConversion<int>();
                e.Property(x => x.Status).HasConversion<int>();
                e.Property(x => x.AmountVnd).IsRequired();

                e.Property(x => x.ProviderTransactionId).HasMaxLength(200).IsRequired();
                e.HasIndex(x => x.ProviderTransactionId).IsUnique();

                e.Property(x => x.Signature).HasMaxLength(500);

                e.HasOne(x => x.Order)
                 .WithMany(o => o.Payments)
                 .HasForeignKey(x => x.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Refunds (1-1 với PaymentTransaction) (có thể remove do out of scope)=====
            modelBuilder.Entity<Refund>(e =>
            {
                e.ToTable("Refunds");
                e.HasKey(x => x.RefundId);
                e.Property(x => x.RefundId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.AmountVnd).IsRequired();
                e.Property(x => x.Reason).HasMaxLength(500).IsRequired();
                e.Property(x => x.ProviderRefundId).HasMaxLength(200).IsRequired();

                e.HasOne(x => x.PaymentTransaction)
                 .WithOne(t => t.Refund)
                 .HasForeignKey<Refund>(x => x.PaymentTransactionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Invoices (rela 1-1 với PaymentTransactions) =====
            modelBuilder.Entity<Invoice>(e =>
            {
                e.ToTable("Invoices");
                e.HasKey(x => x.InvoiceId);
                e.Property(x => x.InvoiceId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.InvoiceNumber).IsUnique();

                e.Property(x => x.SubtotalVnd).IsRequired();
                e.Property(x => x.TaxVnd).IsRequired();
                e.Property(x => x.TotalVnd).IsRequired();

                // khoá ngoại cho người mua
                e.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.PaymentTransaction)
                 .WithOne(t => t.Invoice)
                 .HasForeignKey<Invoice>(x => x.PaymentTransactionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== WebhookLogs (based on Fluent API) =====
            modelBuilder.Entity<WebhookLog>(e =>
            {
                e.ToTable("WebhookLogs");
                e.HasKey(x => x.WebhookLogId);
                e.Property(x => x.WebhookLogId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.EventType).HasMaxLength(100).IsRequired();
                e.Property(x => x.ProviderTransactionId).HasMaxLength(200);
                e.Property(x => x.VerificationError).HasMaxLength(500);
                e.Property(x => x.Payload).HasColumnType("nvarchar(max)");
                e.HasIndex(x => x.OrderCode);
                e.HasIndex(x => x.ProviderTransactionId);
            });
        }
    }
}
