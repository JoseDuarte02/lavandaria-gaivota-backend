// Data/ApplicationDbContext.cs
using LavandariaGaivotaAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LavandariaGaivotaAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        // ... (o seu construtor existente) ...
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // <<< ADICIONE ESTAS LINHAS >>>
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Service> Services { get; set; }

        // ... (pode adicionar OnModelCreating para configurações mais avançadas se necessário) ...
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar a relação entre Order e OrderItem
            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Se um pedido for apagado, os seus itens também são.

            // Configurar a relação entre User e Order
            builder.Entity<ApplicationUser>()
                .HasMany<Order>() // Um ApplicationUser tem muitos Orders
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .IsRequired();

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Se um utilizador for apagado, as suas moradas também são.
        }
    }
}