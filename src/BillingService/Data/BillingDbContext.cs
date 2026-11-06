using Microsoft.EntityFrameworkCore;
using BillingService.Entities;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<LineaFactura> LineasFactura => Set<LineaFactura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Nif).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);

            entity.HasIndex(c => c.Nif).IsUnique();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Precio).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Total).HasPrecision(18, 2);
            entity.Property(f => f.Estado).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(f => f.Cliente)
                  .WithMany(c => c.Facturas)
                  .HasForeignKey(f => f.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LineaFactura>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.PrecioUnitario).HasPrecision(18, 2);

            entity.HasOne(l => l.Factura)
                  .WithMany(f => f.Lineas)
                  .HasForeignKey(l => l.FacturaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Producto)
                  .WithMany(p => p.LineasFactura)
                  .HasForeignKey(l => l.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}