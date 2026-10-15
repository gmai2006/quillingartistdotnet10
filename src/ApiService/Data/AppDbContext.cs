using ApiService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const string Schema = "quillingartist";

    public DbSet<Addresses> Addresses => Set<Addresses>();
    public DbSet<Customers> Customers => Set<Customers>();
    public DbSet<OrderItems> OrderItems => Set<OrderItems>();
    public DbSet<Orders> Orders => Set<Orders>();
    public DbSet<Products> Products => Set<Products>();
    public DbSet<Users> Users => Set<Users>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<LocalTimestampConverter>();
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetConverter>();
    }

    private sealed class LocalTimestampConverter() : ValueConverter<DateTime, DateTime>(
        v => DateTime.SpecifyKind(v.Kind == DateTimeKind.Utc ? v.ToLocalTime() : v, DateTimeKind.Unspecified),
        v => v);

    private sealed class UtcDateTimeOffsetConverter() : ValueConverter<DateTimeOffset, DateTimeOffset>(
        v => v.ToUniversalTime(),
        v => v);

#pragma warning disable EF1002
    public async Task<int> NextIdAsync(string sequence, CancellationToken ct = default)
    {
        return await Database
            .SqlQueryRaw<int>($"select nextval('{Schema}.{sequence}')::int as \"Value\"")
            .SingleAsync(ct);
    }
#pragma warning restore EF1002
}
