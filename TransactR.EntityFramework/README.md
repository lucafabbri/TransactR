# TransactR.EntityFramework

`TransactR.EntityFramework` is a library that provides an `IMementoStore` implementation based on Entity Framework Core, allowing you to persist transaction state directly in the application's database. This library integrates cleanly with an existing `DbContext`, following an approach similar to **OpenIddict**.

## Installation

First, install the NuGet package in your project.

```
dotnet add package TransactR.EntityFramework

```

## Configuration

Configuration involves two simple steps:

### 1. Modifying the `DbContext`

To allow Entity Framework to create the necessary tables for mementos, you need to extend your `DbContext` and add the entities. A clean way to do this is by using the `UseTransactR` extension method in your `DbContextOptionsBuilder`.

```
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your model configuration here...
    }
}

```

Modify the `DbContext` service configuration in your `Program.cs` or `Startup.cs` file to use the `UseTransactR` extension method:

```
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Add the TransactR entities to your model.
    options.UseTransactR<ApplicationDbContext, MyState, int>();
});

```

### 2. Adding Migrations

After configuring your `DbContext`, run the following commands to create a migration that includes the TransactR tables:

```
dotnet ef migrations add AddTransactR
dotnet ef database update

```

This will create the `Mementos` table in your database.

### 3. Service Injection

Finally, register the memento store in the Dependency Injection container:

```
// Program.cs
builder.Services.AddEntityFrameworkMementoStore<ApplicationDbContext, MyState, int>();

```

## Usage Example

After configuration, you can inject `IMementoStore<MyState, int>` into your services or controllers to save and retrieve mementos.

```
public class MyService
{
    private readonly IMementoStore<MyState, int> _mementoStore;

    public MyService(IMementoStore<MyState, int> mementoStore)
    {
        _mementoStore = mementoStore;
    }

    public async Task PerformOperationAsync(string transactionId, MyState state)
    {
        // Save a new memento
        await _mementoStore.SaveAsync(transactionId, 1, state);

        // Retrieve a memento
        var retrievedMemento = await _mementoStore.RetrieveAsync(transactionId, 1);
        
        // Remove a memento
        await _mementoStore.RemoveAsync(transactionId, 1);
    }
}

