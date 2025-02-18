using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.PostgreSQL;

public abstract class PostgresDbContextFactory<T> : DbContextFactory<T> where T : DbContext
{
    public PostgresDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer )
    {
    }

    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return new DbContextOptionsBuilder<DbContextBase>().UseNpgsql(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}