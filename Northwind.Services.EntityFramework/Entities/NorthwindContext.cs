using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Shipper> Shippers { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.OrderId, od.ProductId });
            }
            else
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }
        }
    }
}
