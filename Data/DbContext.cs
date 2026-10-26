using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;


namespace DigitalWallet.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<TransactionEvent> TransactionEvents { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

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

        // Indexes
        modelBuilder.Entity<TransactionEvent>()
            .HasIndex(u => u.WalletId);


        // Wallet
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(19, 4)
            .HasDefaultValue(0.0000);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.UserId)
            .HasMaxLength(50)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<SequentialGuidValueGenerator>();

        modelBuilder.Entity<Wallet>()
            .Property(w => w.Version)
            .IsRowVersion();


        // Wallet Transaction
        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.Amount)
            .HasPrecision(19, 4)
            .HasDefaultValue(0.0000);

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Type)
            .HasConversion<string>();


        // Outbox
        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(om => om.Key);

        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(om => om.Topic);

        modelBuilder.Entity<OutboxMessage>()
            .Property(om => om.Processed)
            .HasDefaultValue(false);

        modelBuilder.Entity<OutboxMessage>()
            .Property(om => om.Key)
            .HasMaxLength(18);

        modelBuilder.Entity<OutboxMessage>()
            .Property(w => w.Topic)
            .HasMaxLength(18);

        modelBuilder.Entity<OutboxMessage>()
            .ComplexProperty(
                om => om.Payload,
                builder =>
                {
                    builder.Property(payload => payload.EventType).HasConversion<string>();
                    builder.Property(payload => payload.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

                }
            );
    }
}