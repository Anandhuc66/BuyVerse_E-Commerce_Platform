using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecommerce_Entity.Models;

namespace Ecommerce_Entity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, AppRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<Category> CategoriesSet { get; set; }
        public DbSet<SubCategory> SubCategoriesSet { get; set; }
        public DbSet<Supplier> SuppliersSet { get; set; }
        public DbSet<Product> ProductsSet { get; set; }
        public DbSet<ProductImage> ProductImagesSet { get; set; }
        public DbSet<Order> OrdersSet { get; set; }
        public DbSet<OrderDetail> OrderDetailsSet { get; set; }
        public DbSet<Payment> PaymentsSet { get; set; }
        public DbSet<Cart> CartsSet { get; set; }
        public DbSet<UserAddress> UserAddressesSet { get; set; }
        public DbSet<ProductReview> ProductReviewsSet { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity table
            builder.Entity<AppRole>().ToTable("AppRoles");

            // Category -> SubCategory -> Product
            builder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier -> Product
            builder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductImage
            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment
            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart: user -> carts
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique index to prevent duplicate (UserId, ProductId)
            builder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            // One-to-one between ApplicationUser and Supplier (supplier as dependent)
            builder.Entity<Supplier>()
                .HasOne(s => s.User)
                .WithOne(u => u.Supplier)
                .HasForeignKey<Supplier>(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull); // if user removed, supplier keeps record (optional)

            // ProductReview relations
            builder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            builder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<OrderDetail>().Property(od => od.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");

            // Unique indexes
            builder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
            builder.Entity<Category>().HasIndex(c => c.Name);
            builder.Entity<SubCategory>().HasIndex(sc => new { sc.CategoryId, sc.Name });

            // Optional: configure max lengths
            builder.Entity<Product>().Property(p => p.SKU).HasMaxLength(50);

            // Performance indexes for frequent queries
            builder.Entity<Order>().HasIndex(o => o.UserId);
            builder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();
            builder.Entity<Product>().HasIndex(p => p.SubCategoryId);
            builder.Entity<ProductReview>().HasIndex(r => r.ProductId);
            builder.Entity<ProductReview>().HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            builder.Entity<Cart>().HasIndex(c => c.UserId);
            builder.Entity<OrderDetail>().HasIndex(od => od.OrderId);

            // Configure cascading for user addresses
            builder.Entity<UserAddress>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
