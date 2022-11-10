using BeachApi.Logging.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachApi.Logging;

public class LoggingDataContext : DbContext
{
    private readonly string connectionString;

    public LoggingDataContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public DbSet<Log> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);

        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Log>(builder =>
        {
            builder.ToTable("Logs");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).UseIdentityColumn(1, 1);

            builder.Property(l => l.Message).HasMaxLength(4000).IsRequired(false);
            builder.Property(l => l.Level).HasConversion<string>().HasMaxLength(50).IsRequired(false);
            builder.Property(l => l.TimeStamp).HasMaxLength(4000).IsRequired(false);
            builder.Property(l => l.Exception).HasMaxLength(4000).IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}