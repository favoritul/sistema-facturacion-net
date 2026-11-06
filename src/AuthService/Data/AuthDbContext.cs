using Microsoft.EntityFrameworkCore;
using AuthService.Entities;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Invitacion> Invitaciones => Set<Invitacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Rol).IsRequired().HasMaxLength(30);

            entity.HasIndex(u => u.Email).IsUnique();
        });
        
        modelBuilder.Entity<Invitacion>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Email).IsRequired().HasMaxLength(150);
            entity.Property(i => i.TokenHash).IsRequired().HasMaxLength(64);

            entity.HasIndex(i => i.TokenHash).IsUnique();
        });
    }
    
}