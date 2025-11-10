using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace DataAccess
{
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

        // DbSet billing (8 bảng)
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanPrice> PlanPrices => Set<PlanPrice>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();
        public DbSet<House> Houses => Set<House>();
        public DbSet<Floor> Floors => Set<Floor>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RoomItem> RoomItems => Set<RoomItem>();
        public DbSet<RoomItemInRoom> RoomItemInRooms => Set<RoomItemInRoom>();
        public DbSet<SubItem> SubItems => Set<SubItem>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<OTPEmail>().ToTable("OTPEmail");

            // GUID mặc định
            modelBuilder.Entity<User>().Property(u => u.UserId).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<OTPEmail>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<ChatMessage>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<ChatSession>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            // Quan hệ User-Role (đã có)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(255);

            // ===== Plans =====
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

            // ===== PlanPrices =====
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

            // ===== Refunds (1-1 với PaymentTransaction) =====
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

            // ===== Invoices (1-1 với PaymentTransaction) =====
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

                // FK tới User (người mua)
                e.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.PaymentTransaction)
                 .WithOne(t => t.Invoice)
                 .HasForeignKey<Invoice>(x => x.PaymentTransactionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== WebhookLogs =====
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

            modelBuilder.Entity<House>(e =>
            {
                e.ToTable("House");
                e.HasKey(x => x.HouseId);
                e.Property(x => x.HouseId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Name).HasMaxLength(255).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany(u => u.Houses)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.UserId);
            });

            // --- Floor ---
            modelBuilder.Entity<Floor>(e =>
            {
                e.ToTable("Floor");
                e.HasKey(x => x.FloorId);
                e.Property(x => x.FloorId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Name).HasMaxLength(255).IsRequired();

                e.HasOne(x => x.House)
                    .WithMany(h => h.Floors)
                    .HasForeignKey(x => x.HouseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // mỗi tầng duy nhất trong cùng 1 house
                e.HasIndex(x => new { x.HouseId, x.Level }).IsUnique();
                e.HasIndex(x => x.HouseId);
            });

            // --- Room ---
            modelBuilder.Entity<Room>(e =>
            {
                e.ToTable("Room");
                e.HasKey(x => x.RoomId);
                e.Property(x => x.RoomId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Name).HasMaxLength(255).IsRequired();
                e.Property(x => x.Type).HasMaxLength(50).IsRequired();

                e.HasOne(x => x.Floor)
                    .WithMany(f => f.Rooms)
                    .HasForeignKey(x => x.FloorId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.FloorId);
            });

            // --- RoomItem ---
            modelBuilder.Entity<RoomItem>(e =>
            {
                e.ToTable("RoomItem");
                e.HasKey(x => x.RoomItemId);
                e.Property(x => x.RoomItemId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Item).HasMaxLength(100).IsRequired();

                // THÊM DÒNG NÀY
                e.Property(x => x.SubName).HasMaxLength(200);

                e.Property(x => x.RoomType).HasMaxLength(50);
                e.HasIndex(x => x.Item);

            });

            // RoomItemInRooms (n–n Room <-> RoomItem)
            modelBuilder.Entity<RoomItemInRoom>(e =>
            {
                e.ToTable("RoomItemInRooms");
                e.Property(x => x.RoomItemId).HasDefaultValueSql("NEWID()");
                e.HasKey(x => new { x.RoomId, x.RoomItemId });

                e.HasOne(x => x.Room)
                 .WithMany(r => r.RoomItemPlacements)
                 .HasForeignKey(x => x.RoomId)
                 .OnDelete(DeleteBehavior.Cascade);     

                e.HasOne(x => x.RoomItem)
                 .WithMany(ri => ri.RoomPlacements)
                 .HasForeignKey(x => x.RoomItemId)
                 .OnDelete(DeleteBehavior.Restrict);    
            });

            modelBuilder.Entity<SubItem>()
            .HasOne(s => s.Placement)
            .WithMany(p => p.SubItems)
            .HasForeignKey(s => new { s.RoomId, s.RoomItemId })
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomItem>()
        .Property(x => x.DefaultX)
        .HasPrecision(18, 2);   // hoặc .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<RoomItem>()
                .Property(x => x.DefaultY)
                .HasPrecision(18, 2);

            // RoomItemInRoom (placement)
            modelBuilder.Entity<RoomItemInRoom>()
                .Property(x => x.X)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RoomItemInRoom>()
                .Property(x => x.Y)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Messages)
            .WithOne(m => m.ChatSession)
            .HasForeignKey(m => m.ChatSessionId);
        }
    }
}