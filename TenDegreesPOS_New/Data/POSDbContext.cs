using Microsoft.EntityFrameworkCore;
using TenDegreesPOS_New.Models;

namespace TenDegreesPOS_New.Data
{
    public class POSDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<StoreSettings> StoreSettings { get; set; }

        // Add these with your other DbSet declarations
        public DbSet<OrderCancellation> OrderCancellations { get; set; }
        public DbSet<CancelledOrderItem> CancelledOrderItems { get; set; }

        // NEW: Supplier Management DbSets
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SupplierIngredient> SupplierIngredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TenDegreesPOS", "pos.db");

            string? directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing configurations...
            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Ingredient)
                .WithMany(i => i.ProductIngredients)
                .HasForeignKey(pi => pi.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionItem>()
                .HasOne(ti => ti.Product)
                .WithMany(p => p.TransactionItems)
                .HasForeignKey(ti => ti.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventory)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Ingredient)
                .WithMany()
                .HasForeignKey(i => i.IngredientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionNumber)
                .IsUnique();

            // Supplier configurations
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique();
                entity.Property(s => s.IsActive).HasDefaultValue(true);
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // SupplierIngredient configuration
            modelBuilder.Entity<SupplierIngredient>(entity =>
            {
                entity.HasKey(si => si.Id);

                entity.HasOne(si => si.Supplier)
                    .WithMany()
                    .HasForeignKey(si => si.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(si => si.Ingredient)
                    .WithMany()
                    .HasForeignKey(si => si.IngredientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(si => new { si.SupplierId, si.IngredientId })
                    .IsUnique();
            });

            // PurchaseOrder configurations
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasIndex(po => po.OrderNumber).IsUnique();
                entity.Property(po => po.Status).HasDefaultValue("Processing");
                entity.Property(po => po.TotalAmount).HasColumnType("DECIMAL");
                entity.Property(po => po.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(po => po.Supplier)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(po => po.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseOrderItem configurations
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.Property(poi => poi.QuantityOrdered).HasColumnType("DECIMAL");
                entity.Property(poi => poi.UnitPrice).HasColumnType("DECIMAL");
                entity.Property(poi => poi.TotalPrice).HasColumnType("DECIMAL");

                entity.HasOne(poi => poi.PurchaseOrder)
                    .WithMany(po => po.PurchaseOrderItems)
                    .HasForeignKey(poi => poi.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(poi => poi.Ingredient)
                    .WithMany()
                    .HasForeignKey(poi => poi.IngredientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== NEW: Order Cancellation Configurations ==========

            modelBuilder.Entity<OrderCancellation>(entity =>
            {
                entity.HasIndex(oc => oc.TransactionNumber).IsUnique();
                entity.HasIndex(oc => oc.CancelledDate);

                entity.Property(oc => oc.OriginalTotal).HasColumnType("DECIMAL");
                entity.Property(oc => oc.RefundAmount).HasColumnType("DECIMAL");
                entity.Property(oc => oc.CancellationType).HasMaxLength(20).HasDefaultValue("Full");
                entity.Property(oc => oc.RefundMethod).HasMaxLength(20).HasDefaultValue("Original");

                entity.HasOne(oc => oc.Transaction)
                    .WithMany()
                    .HasForeignKey(oc => oc.TransactionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oc => oc.CancelledByUser)
                    .WithMany()
                    .HasForeignKey(oc => oc.CancelledByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CancelledOrderItem>(entity =>
            {
                entity.Property(coi => coi.UnitPrice).HasColumnType("DECIMAL");
                entity.Property(coi => coi.TotalPrice).HasColumnType("DECIMAL");

                entity.HasOne(coi => coi.Cancellation)
                    .WithMany()
                    .HasForeignKey(coi => coi.CancellationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}