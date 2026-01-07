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

    [Fact]
    public void ApplyOptions_CalledMultipleTimes_ReturnsNewOptionsEachTime()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options1 = factory.TestApplyOptions();
        var options2 = factory.TestApplyOptions();

        Assert.NotNull(options1);
        Assert.NotNull(options2);
        Assert.NotSame(options1, options2);
    }

    [Fact]
    public void ApplyOptions_WithDifferentConnectionStrings_UsesCorrectConnectionString()
    {
        var connectionString1 = "Host=server1;Database=db1;Username=user1;Password=pass1";
        var connectionString2 = "Host=server2;Database=db2;Username=user2;Password=pass2";
        
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.SetupSequence(x => x.AdminConnectionString)
            .Returns(connectionString1)
            .Returns(connectionString2);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        
        var options1 = factory.TestApplyOptions();
        var options2 = factory.TestApplyOptions();

        Assert.NotNull(options1);
        Assert.NotNull(options2);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.Exactly(2));
    }

    [Fact]
    public void Constructor_InheritsFromDbContextFactory()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        Assert.IsAssignableFrom<DbContextFactory<TestDbContext>>(factory);
    }

    [Fact]
    public void ApplyOptions_OptionsHaveCorrectType()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.IsType<DbContextOptions<DbContextBase>>(options);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyOptions_WithBothSensitiveDataLoggingValues_ReturnsValidOptions(bool sensitiveDataLogging)
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Host=localhost;Database=test;Username=test;Password=test");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions(sensitiveDataLogging);

        Assert.NotNull(options);
        Assert.IsType<DbContextOptions<DbContextBase>>(options);
    }

    [Fact]
    public void ApplyOptions_WithEmptyConnectionString_StillReturnsOptions()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(string.Empty);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithComplexConnectionString_ReturnsValidOptions()
    {
        var complexConnectionString = "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass;Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionLifetime=15;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(complexConnectionString);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestPostgresDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }
}
