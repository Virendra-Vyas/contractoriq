using ContractorIQ.API.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace ContractorIQ.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ContractorProfile> Profiles => Set<ContractorProfile>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Ir35Analysis> Ir35Analyses => Set<Ir35Analysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        // ── User ────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ── ContractorProfile — one-to-one with User ────────────────────────
        modelBuilder.Entity<ContractorProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<ContractorProfile>(p => p.UserId);

        modelBuilder.Entity<ContractorProfile>()
            .Property(p => p.ProfileEmbedding)
            .HasColumnType("vector(1536)");

        // ── Job ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Job>()
            .Property(j => j.DescriptionEmbedding)
            .HasColumnType("vector(1536)");

        modelBuilder.Entity<Job>()
            .HasIndex(j => new { j.ExternalId, j.Source })
            .IsUnique();

        // ── Subscription — one-to-one with User ─────────────────────────────
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasOne(s => s.User)
             .WithOne(u => u.Subscription)
             .HasForeignKey<Subscription>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(s => s.Tier)
             .HasDefaultValue("free");

            e.Property(s => s.Status)
             .HasDefaultValue("active");

            e.Property(s => s.CreatedAt)
             .HasDefaultValueSql("NOW()");

            e.Property(s => s.UpdatedAt)
             .HasDefaultValueSql("NOW()");

            e.HasIndex(s => s.UserId)
             .IsUnique();

            e.HasIndex(s => s.StripeSubscriptionId);
            e.HasIndex(s => s.StripeCustomerId);
        });

        // ── Application ─────────────────────────────────────────────────────
        modelBuilder.Entity<Application>()
            .HasOne(a => a.User)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId);

        // ── IR35 Analysis ────────────────────────────────────────────────────
        modelBuilder.Entity<Ir35Analysis>()
            .HasOne(a => a.Job)
            .WithOne(j => j.Ir35Analysis)
            .HasForeignKey<Ir35Analysis>(a => a.JobId);
    }
}