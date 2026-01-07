using Bounteous.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bounteous.Data.PostgreSQL.Tests;

public class PostgresDbContextFactoryTests
{
    private class TestDbContext : DbContextBase, IDbContext
    {
        public TestDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer) : base(options, observer)
        {
        }

        protected override void RegisterModels(ModelBuilder modelBuilder)
        {
        }
    }

    private class TestPostgresDbContextFactory : PostgresDbContextFactory<TestDbContext>
    {
        public TestPostgresDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
            : base(connectionBuilder, observer)
        {
        }

        protected override TestDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        {
            return new TestDbContext(options, observer);
        }

        public DbContextOptions<DbContextBase> TestApplyOptions(bool sensitiveDataLoggingEnabled = false)
        {
            return ApplyOptions(sensitiveDataLoggingEnabled);
        }
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        Assert.NotNull(factory);
    }

    [Fact]
    public void ApplyOptions_WithDefaultParameters_ReturnsConfiguredOptions()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingEnabled_ReturnsConfiguredOptions()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: true);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingDisabled_ReturnsConfiguredOptions()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: false);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_SetsNpgsqlLegacyTimestampBehavior()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        factory.TestApplyOptions();

        var switchValue = AppContext.TryGetSwitch("Npgsql.EnableLegacyTimestampBehavior", out var isEnabled);
        Assert.True(switchValue);
        Assert.True(isEnabled);
    }

    [Fact]
    public void ApplyOptions_UsesConnectionStringFromConnectionBuilder()
    {
        var expectedConnectionString = "Host=testhost;Database=testdb;Username=testuser;Password=testpass";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(expectedConnectionString);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }
}
