using DigitalWallet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace DigitalWallet.Data;

internal class DatabaseContext : DbContext
{
    private const string DefaultCurrency = "NGN";

    public DbSet<Wallet> Wallets = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Wallet
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(12, 4)
            .HasDefaultValue(0.0000);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.UserId)
            .HasMaxLength(18)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<SequentialGuidValueGenerator>();

        // Configure Wallet Default currency value
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Currency)
            .HasDefaultValue("NGN");

        // Dates
        modelBuilder.Entity<Wallet>()
            .Property(w => w.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();
    }
}