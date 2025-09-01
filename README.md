# TransactR
[![Build Status](https://github.com/lucafabbri/TransactR/actions/workflows/main.yml/badge.svg)](https://github.com/lucafabbri/TransactR/actions) [![GitHub release](https://img.shields.io/github/v/release/lucafabbri/TransactR)](https://github.com/lucafabbri/TransactR/releases) [![NuGet](https://img.shields.io/nuget/v/TransactR)](https://www.nuget.org/packages/TransactR)

**TransactR** is a lightweight and flexible .NET library for managing transactions and rollbacks based on the Memento pattern. It is designed to be integrated into command processing pipelines, such as those provided by MediatR or Concordia.Core, to save the state of an operation before its execution and automatically restore it in case of an error.

## Key Concepts

* **Memento Pattern**: The core of the library. It captures and stores an object's internal state (the "memento") so that it can be restored later, without violating encapsulation.

* **Transactional Behavior**: A pipeline behavior that intercepts requests, orchestrates the saving of the initial state, and handles rollback logic within a `try/catch` block.

* **State Management**: State persistence is managed through the `IMementoStore` interface, while the restoration logic is delegated to an `IStateRestorer`, allowing for maximum flexibility (in-memory, database, etc.).

* **Interactive Saga**: Supports multi-step processes where the transaction state is preserved across multiple calls, enabling the continuation of complex workflows.

* **Disaster Recovery**: Offers configurable rollback policies (`RollbackToCurrentStep`, `RollbackToBeginning`, `DeleteTransactionState`) to handle unexpected exceptions.

## Installation

The library is split into packages to maintain modularity. Install the Core package and your desired integration and persistence packages.

```shell
dotnet add package TransactR
dotnet add package TransactR.MediatR� # or TransactR.Concordia

```

## Memento Store Implementations

`TransactR` is designed to be storage-agnostic. You can use one of the following official implementations or create your own by implementing the `IMementoStore` interface.

### TransactR.EntityFramework

This store is ideal for applications that already use Entity Framework Core. It integrates seamlessly into an existing `DbContext`, leveraging its transaction management and allowing developers to handle migrations.

* **Installation:**
    ```shell
    dotnet add package TransactR.EntityFramework
    ```
* **DI Integration:**
    ```csharp
    // Program.cs
    // Adds TransactR entities to your DbContext and registers the MementoStore implementation.
    builder.Services.AddEntityFrameworkMementoStore<ApplicationDbContext, MyState, int>();
    ```

### TransactR.DistributedMemoryCache

This implementation is perfect for high-speed, volatile state persistence. It's a great choice for short-lived transactions or for use in microservices where a distributed cache like Redis is already in place.

* **Installation:**
    ```shell
    dotnet add package TransactR.DistributedMemoryCache
    ```
* **DI Integration:**
    ```csharp
    // Program.cs
    // The service automatically handles serialization and deserialization.
    builder.Services.AddDistributedMemoryCache(); // Or a distributed cache of your choice (e.g., Redis)
    builder.Services.AddDistributedMemoryCacheMementoStore<MyState, int>();
    ```

### TransactR.MongoDB

This store offers maximum flexibility for state persistence. It's ideal for complex mementos where the structure may change over time, as it doesn't require schema migrations.

* **Installation:**
    ```shell
    dotnet add package TransactR.MongoDB
    ```
* **DI Integration:**
    ```csharp
    // Program.cs
    builder.Services.AddMongoDbMementoStore<MyState, int>(
        builder.Configuration.GetConnectionString("MongoDbConnection"),
        "your-database-name",
        "mementos"
    );
    ```

### TransactR.AzureTableStorage

This is a highly performant and cost-effective store for large-scale applications. It's an excellent choice for scenarios that require a simple key-value store with high throughput and low latency.

* **Installation:**
    ```shell
    dotnet add package TransactR.AzureTableStorage
    ```
* **DI Integration:**
    ```csharp
    // Program.cs
    builder.Services.AddAzureTableStorageMementoStore<MyState, int>(
        builder.Configuration.GetConnectionString("AzureStorageConnectionString")
    );
    ```

## Usage Example (with MediatR)

### 1. Configuration (Dependency Injection)

```csharp
// Program.cs
builder.Services.AddMediatR(cfg => { /* ... */ });

// Adds the behavior and the context provider
builder.Services.AddTransactRMediatR();�

// Register your implementations
builder.Services.AddSingleton<IMementoStore<MyState, MyStep>, InMemoryMementoStore<MyState, MyStep>>();
builder.Services.AddScoped<IStateRestorer<MyState>, MyStateRestorer>();

```

### 2. Component Definitions

```csharp
// The command that triggers the transaction
public class MyCommand : IRequest, ITransactionalRequest<MyState, MyStep>
{
� � public string TransactionId { get; set; }
}

// The context that manages the transaction's state and logic
public class MyContext : TransactionContext<MyContext, MyState, MyStep>
{
� � public override MyStep InitialStep => MyStep.Start;

� � public override TransactionOutcome EvaluateResponse<TResponse>(TResponse response)
� � {
� � � � // Logic to decide if the transaction is completed, in progress, or failed
� � � � return TransactionOutcome.Completed;
� � }
}

// The logic to restore the state in case of an error
public class MyStateRestorer : IStateRestorer<MyState>
{
� � public Task RestoreAsync(MyState state, CancellationToken cancellationToken)
� � {
� � � � // Logic to update the database with the previous state
� � � � Console.WriteLine($"Restoring state value to: {state.Value}");
� � � � return Task.CompletedTask;
� � }
}

```

### 3. Usage in the Handler

```csharp
public class MyCommandHandler : IRequestHandler<MyCommand>
{
� � private readonly ITransactionContextProvider<MyContext> _contextProvider;

� � public MyCommandHandler(ITransactionContextProvider<MyContext> contextProvider)
� � {
� � � � _contextProvider = contextProvider;
� � }

� � public Task Handle(MyCommand request, CancellationToken cancellationToken)
� � {
� � � � // Access the context automatically created by the behavior
� � � � var context = _contextProvider.Context;
� � � ��
� � � � // Execute your business logic
� � � � context.State.Value = 100;

� � � � // If an exception is thrown here, IStateRestorer.RestoreAsync will be invoked.

� � � � return Task.CompletedTask;
� � }
}

```

## Usage Example (with Concordia.Core)

### 1. Configuration (Dependency Injection)

`TransactR.Concordia` automatically registers the `TransactionalBehavior` and requires that the `IMementoStore` and `IStateRestorer` be registered separately.

```csharp
// Program.cs
builder.Services.AddMediator(cfg => { /*...*/ });
builder.Services.AddTransactRConcordia();

// Register your implementations
builder.Services.AddEntityFrameworkMementoStore<ApplicationDbContext, MyState, int>(builder.Configuration);
builder.Services.AddScoped<IStateRestorer<MyState>, MyStateRestorer>();

```

### 2. Component Definitions

The components for `Concordia.Core` are similar to those for MediatR, but they use the `ICommand` interface and `TransactRConcordiaContext` to carry the transaction context.

```csharp
// The command that triggers the transaction
public class MyCommand : ICommand<Response>, ITransactionalRequest<MyState, MyStep>
{
    public string TransactionId { get; set; }
}

// The handler's context that carries the transaction state.
// The base class handles context creation and state management.
public class MyConcordiaContext : TransactRConcordiaContext<MyContext> { }

// The logic to restore the state in case of an error
public class MyStateRestorer : IStateRestorer<MyState>
{
    public Task RestoreAsync(MyState state, CancellationToken cancellationToken)
    {
        // Logic to update the database with the previous state
        Console.WriteLine($"Restoring state value to: {state.Value}");
        return Task.CompletedTask;
    }
}

```

### 3. Usage in the Handler

In `Concordia.Core`, the `ITransactionContext` is available in the handler via `context.TransactionContext`.

```csharp
public class MyCommandHandler : ICommandHandler<MyCommand, MyConcordiaContext>
{
    public Task<Response> HandleAsync(MyCommand request, MyConcordiaContext context, CancellationToken cancellationToken)
    {
        // Access the context automatically created by the behavior
        var transactionContext = context.TransactionContext;

        // Execute your business logic
        transactionContext.State.Value = 100;

        // If an exception is thrown here, IStateRestorer.RestoreAsync will be invoked.
        return Task.FromResult(new Response());
    }
}
