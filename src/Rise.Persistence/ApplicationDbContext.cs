using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Products;
using Rise.Domain.Projects;

namespace Rise.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : IdentityDbContext<IdentityUser>(opts)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Technician> Technicians => Set<Technician>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // REMOVE the global 4000-char max — it blows up MySQL row size
        // configurationBuilder.Properties<string>().HaveMaxLength(4_000);

        // Keep decimal precision
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MySQL/MariaDB charset & collation
        modelBuilder.HasCharSet("utf8mb4");
        modelBuilder.UseCollation("utf8mb4_general_ci"); // or utf8mb4_0900_ai_ci on MySQL 8+

        // ---- Identity overrides to avoid 3072-byte key overflows under utf8mb4 ----
        const int IdLen = 191;
        const int NormLen = 256;
        const int LoginLen = 128;

        modelBuilder.Entity<IdentityUser>(b =>
        {
            b.Property(u => u.Id).HasMaxLength(IdLen);
            b.Property(u => u.UserName).HasMaxLength(NormLen);
            b.Property(u => u.NormalizedUserName).HasMaxLength(NormLen);
            b.Property(u => u.Email).HasMaxLength(NormLen);
            b.Property(u => u.NormalizedEmail).HasMaxLength(NormLen);
        });

        modelBuilder.Entity<IdentityRole>(b =>
        {
            b.Property(r => r.Id).HasMaxLength(IdLen);
            b.Property(r => r.Name).HasMaxLength(NormLen);
            b.Property(r => r.NormalizedName).HasMaxLength(NormLen);
        });

        modelBuilder.Entity<IdentityUserRole<string>>(b =>
        {
            b.Property(ur => ur.UserId).HasMaxLength(IdLen);
            b.Property(ur => ur.RoleId).HasMaxLength(IdLen);
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.Property(uc => uc.UserId).HasMaxLength(IdLen);
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.Property(rc => rc.RoleId).HasMaxLength(IdLen);
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.Property(l => l.UserId).HasMaxLength(IdLen);
            b.Property(l => l.LoginProvider).HasMaxLength(LoginLen);
            b.Property(l => l.ProviderKey).HasMaxLength(LoginLen);
        });

        modelBuilder.Entity<IdentityUserToken<string>>(b =>
        {
            b.Property(t => t.UserId).HasMaxLength(IdLen);
            b.Property(t => t.LoginProvider).HasMaxLength(LoginLen);
            b.Property(t => t.Name).HasMaxLength(LoginLen);
        });

        // Apply the rest of your entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
