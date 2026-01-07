# Bounteous.Data.PostgreSQL

A specialized Entity Framework Core data access library for PostgreSQL databases in .NET 8+ applications. This library extends the base `Bounteous.Data` functionality with PostgreSQL-specific configurations, optimizations, and database provider settings including snake_case naming conventions.

## 📦 Installation

Install the package via NuGet:

```bash
dotnet add package Bounteous.Data.PostgreSQL
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data.PostgreSQL
```

## 🚀 Quick Start

### 1. Configure Services

```csharp
using Bounteous.Data.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register the module
    services.AddModule<ModuleStartup>();
    
    // Register your connection string provider
    services.AddSingleton<IConnectionStringProvider, MyConnectionStringProvider>();
    
    // Register your PostgreSQL DbContext factory
    services.AddScoped<IDbContextFactory<MyDbContext>, MyDbContextFactory>();
}
```

### 2. Create Your PostgreSQL DbContext Factory

```csharp
using Bounteous.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;

public class MyDbContextFactory : PostgresDbContextFactory<MyDbContext>
{
    public MyDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override MyDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
    {
        return new MyDbContext(options, observer);
    }
}
```

### 3. Configure Connection String Provider

```csharp
using Bounteous.Data;
using Microsoft.Extensions.Configuration;

public class MyConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public MyConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ConnectionString => _configuration.GetConnectionString("PostgreSQLConnection") 
        ?? throw new InvalidOperationException("PostgreSQL connection string not found");
}
```

### 4. Use Your PostgreSQL Context

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var customer = new Customer 
        { 
            Name = name, 
            Email = email 
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        return customer;
    }
}
```

## 🏗️ Architecture Overview

Bounteous.Data.PostgreSQL builds upon the foundation of `Bounteous.Data` and provides PostgreSQL-specific enhancements:

- **PostgreSQL Provider Integration**: Uses `Npgsql.EntityFrameworkCore.PostgreSQL` for optimal PostgreSQL performance
- **Snake Case Naming**: Automatic snake_case naming convention support
- **Legacy Timestamp Behavior**: Handles PostgreSQL timestamp behavior compatibility
- **Connection Resilience**: Built-in retry policies for PostgreSQL connection failures
- **PostgreSQL-Specific Optimizations**: Configured for PostgreSQL's unique characteristics
- **Audit Trail Support**: Inherits automatic auditing from base `Bounteous.Data`
- **Soft Delete Support**: Logical deletion capabilities optimized for PostgreSQL

## 🔧 Key Features

### PostgreSQL-Specific DbContext Factory

The `PostgresDbContextFactory<T>` class provides PostgreSQL-optimized configuration:

```csharp
public abstract class PostgresDbContextFactory<T> : DbContextFactory<T> where T : IDbContext
{
    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        // Enable legacy timestamp behavior for compatibility
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return new DbContextOptionsBuilder<DbContextBase>()
            .UseNpgsql(ConnectionBuilder.AdminConnectionString, sqlOptions => 
            { 
                sqlOptions.EnableRetryOnFailure(); 
            })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}
```

**Features:**
- **Snake Case Naming**: Automatic conversion to PostgreSQL snake_case convention
- **Legacy Timestamp Behavior**: Ensures compatibility with PostgreSQL timestamp handling
- **Retry on Failure**: Automatic retry for transient PostgreSQL connection issues
- **Sensitive Data Logging**: Configurable logging for debugging (disabled in production)
- **Detailed Errors**: Enhanced error reporting for development
- **PostgreSQL Provider**: Uses official Npgsql Entity Framework provider

### Connection Management

PostgreSQL-specific connection handling with built-in resilience:

```csharp
// Connection string format for PostgreSQL
"Host=localhost;Database=MyDatabase;Username=username;Password=password;"

// With additional PostgreSQL-specific options
"Host=localhost;Database=MyDatabase;Username=username;Password=password;" +
"Port=5432;" +
"SSL Mode=Require;" +
"Connection Lifetime=0;" +
"Command Timeout=30;"
```

### Snake Case Naming Convention

The library automatically applies snake_case naming conventions:

```csharp
// C# property names
public class Customer : AuditImmutableBase
{
    public string FirstName { get; set; }  // Maps to first_name
    public string LastName { get; set; }   // Maps to last_name
    public DateTime CreatedOn { get; set; } // Maps to created_on
}

// Database table and column names
// Table: customers
// Columns: id, first_name, last_name, created_on, created_by, etc.
```

## 📚 Usage Examples

### Basic CRUD Operations with PostgreSQL

```csharp
public class ProductService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public ProductService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = new Product 
        { 
            Name = name, 
            Price = price,
            CreatedOn = DateTime.UtcNow
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        return product;
    }

    public async Task<List<Product>> GetProductsAsync(int page = 1, int size = 50)
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedOn)
            .ToPaginatedListAsync(page, size);
    }

    public async Task<Product> UpdateProductAsync(Guid productId, string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = await context.Products.FindById(productId);
        product.Name = name;
        product.Price = price;
        
        await context.SaveChangesAsync();
        return product;
    }
}
```

### PostgreSQL-Specific Query Operations

```csharp
public class OrderService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public OrderService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .SumAsync(o => o.TotalAmount);
    }

    // PostgreSQL-specific JSON operations
    public async Task<List<Order>> GetOrdersWithMetadataAsync(string metadataKey)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => EF.Functions.JsonContains(o.Metadata, $"\"{metadataKey}\""))
            .Where(o => !o.IsDeleted)
            .ToListAsync();
    }
}
```

### Soft Delete Operations

```csharp
public async Task DeleteProductAsync(Guid productId, Guid userId)
{
    using var context = _contextFactory.Create().WithUserId(userId);
    
    var product = await context.Products.FindById(productId);
    
    // Soft delete - sets IsDeleted = true
    product.IsDeleted = true;
    
    await context.SaveChangesAsync();
}
```

## 🔧 Configuration Options

### PostgreSQL Connection String Options

```csharp
// Basic connection string
"Host=localhost;Database=MyDatabase;Username=username;Password=password;"

// With additional PostgreSQL options
"Host=localhost;Database=MyDatabase;Username=username;Password=password;" +
"Port=5432;" +
"SSL Mode=Require;" +
"Connection Lifetime=0;" +
"Command Timeout=30;" +
"Pooling=true;" +
"MinPoolSize=0;" +
"MaxPoolSize=100;"
```

### PostgreSQL-Specific DbContext Configuration

```csharp
public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure PostgreSQL-specific settings
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("numeric(18,2)");
            
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("numeric(18,2)");
            
        // Configure JSON columns
        modelBuilder.Entity<Order>()
            .Property(o => o.Metadata)
            .HasColumnType("jsonb");
    }
}
```

### Advanced PostgreSQL Features

```csharp
// Using PostgreSQL-specific data types
public class Document : AuditImmutableBase
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string[] Tags { get; set; } // Maps to PostgreSQL array
    public Dictionary<string, object> Metadata { get; set; } // Maps to jsonb
}

// Configuration for PostgreSQL arrays and JSON
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<Document>()
        .Property(d => d.Tags)
        .HasColumnType("text[]");
        
    modelBuilder.Entity<Document>()
        .Property(d => d.Metadata)
        .HasColumnType("jsonb");
}
```

## 🎯 Target Framework

- **.NET 8.0** and later

## 📋 Dependencies

- **Bounteous.Data** (0.0.6) - Base data access functionality
- **Microsoft.EntityFrameworkCore** (9.0.3) - Entity Framework Core
- **Npgsql.EntityFrameworkCore.PostgreSQL** (9.0.4) - PostgreSQL provider for EF Core
- **EntityFrameworkCore.NamingConventions** (8.0.0) - Naming convention support
- **Microsoft.Extensions.Configuration.Abstractions** (9.0.3) - Configuration management

## 🔗 Related Projects

- [Bounteous.Data](../Bounteous.Data/) - Base data access library
- [Bounteous.Core](../Bounteous.Core/) - Core utilities and patterns
- [Bounteous.Data.MySQL](../Bounteous.Data.MySQL/) - MySQL-specific implementation

## 🤝 Contributing

This library is maintained by Xerris Inc. For contributions, please contact the development team.

## 📄 License

See [LICENSE](LICENSE) file for details.

---

*This library provides PostgreSQL-specific enhancements to the Bounteous.Data framework, ensuring optimal performance and compatibility with PostgreSQL databases including snake_case naming conventions in enterprise .NET applications.*