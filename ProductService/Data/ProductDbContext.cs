using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }

        // Optional: Cấu hình độ chính xác cho Price
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // decimal(18,2)
        }
    }
}
