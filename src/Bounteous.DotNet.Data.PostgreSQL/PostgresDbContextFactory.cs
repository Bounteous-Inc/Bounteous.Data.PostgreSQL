using Microsoft.EntityFrameworkCore;
using Xerris.DotNet.Data;

namespace Bounteous.DotNet.Data.PostgreSQL;

public abstract class PostgresDbContextFactory<T> : DbContextFactory<T> where T : DbContext
{
    public PostgresDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer )
    {
    }

    protected override DbContextOptions<T> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return new DbContextOptionsBuilder<T>().UseNpgsql(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}