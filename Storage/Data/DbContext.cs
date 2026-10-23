using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace DigitalWallet.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Wallet> Wallets = null!;
    public DbSet<WalletTransaction> WalletTransactions = null!;
    public DbSet<TransactionEvent> TransactionEvents = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relationships
        modelBuilder.Entity<Wallet>()
            .HasMany(w => w.Transactions)
            .WithOne(wt => wt.Wallet)
            .HasForeignKey(wt => wt.WalletId)
            .IsRequired();

        // Wallet
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(19, 4)
            .HasDefaultValue(0.0000);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.UserId)
            .HasMaxLength(18)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<SequentialGuidValueGenerator>();

        modelBuilder.Entity<Wallet>()
            .Property(w => w.Version)
            .IsRowVersion();


        // Dates
        modelBuilder.Entity<Wallet>()
            .Property(w => w.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();

        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();

        // Enums
        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Type)
            .HasConversion<string>();

        // Complex Types
        modelBuilder.Entity<TransactionEvent>(ebuilder => { ebuilder.ComplexProperty(te => te.EventData); });
    }
}