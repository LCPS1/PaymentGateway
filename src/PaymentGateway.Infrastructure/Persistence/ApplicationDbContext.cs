using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Merchants.Entities;
using PaymentGateway.Domain.Payments.Entities;

namespace PaymentGateway.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Merchant> Merchants { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Merchant>().ToTable("Merchants");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}