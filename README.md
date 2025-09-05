# TransactR
[![Build Status](https://github.com/lucafabbri/TransactR/actions/workflows/main.yml/badge.svg)](https://github.com/lucafabbri/TransactR/actions) [![GitHub release](https://img.shields.io/github/v/release/lucafabbri/TransactR)](https://github.com/lucafabbri/TransactR/releases) [![NuGet](https://img.shields.io/nuget/v/TransactR)](https://www.nuget.org/packages/TransactR)

A lightweight .NET library for building reliable, stateful, and multi-step operations using the Memento pattern.

---

## 🤔 Why TransactR?

Modern applications often deal with complex business processes that span multiple service calls or user interactions. Managing the state of these operations and ensuring data consistency in case of failure can be challenging.

**TransactR** simplifies this by providing a transactional layer for your command pipeline (like MediatR or Concordia.Core). It allows you to:

* **Implement Sagas**: Easily build long-running processes where state is preserved between steps.
* **Prevent Inconsistent Data**: Automatically roll back to a previous valid state when an operation fails, whether due to a system exception or a business logic failure.
* **Decouple State Management**: Keep your business logic clean by abstracting away the persistence and restoration of state.

## ✨ Features

* **State Management with Memento Pattern**: Captures an object's state to allow for later restoration.
* **Transactional Pipeline Behavior**: Intercepts command processing to orchestrate state saving, execution, and rollback.
* **Pluggable Storage**: Abstracted persistence via `IMementoStore` with multiple backends (Entity Framework, MongoDB, Redis, etc.).
* **Custom Rollback Logic**: Define how to restore state when an operation fails using `IStateRestorer`.
* **Interactive Saga Support**: Maintains transaction state across multiple requests.
* **Flexible Outcome Evaluation**: Determines transaction outcome by evaluating the handler's response, not just by catching exceptions.
* **Configurable Disaster Recovery**: Offers fine-grained rollback policies (`RollbackToCurrentStep`, `RollbackToBeginning`, `DeleteTransactionState`).
* **Per-Request Rollback Policies**: Override the default rollback behavior directly on your request object.

## ⚙️ How It Works

1.  A command implementing `ITransactionalRequest` enters the pipeline.
2.  The `TransactionalBehavior` intercepts it.
3.  It retrieves the transaction's current state from the `IMementoStore` using the `TransactionId`.
4.  It creates a `TransactionContext` containing the state.
5.  The business logic is executed in the command handler, which returns a response.
6.  The `TransactionContext` evaluates the response. If the outcome is `Failed`, or if an unhandled exception occurs, a rollback is triggered.
7.  The `IStateRestorer` is invoked to restore the previous state based on the configured `RollbackPolicy`.
8.  The final state is persisted back to the `IMementoStore` if the transaction is still in progress.

## 🚀 Getting Started: Example with MediatR

Here is a complete example of how to configure and use TransactR.

### 1. Define State, Steps, and Response

First, define the objects for your transaction's state, workflow steps, and the response from your handler.

```csharp
// The state object that will be saved and restored.
public class MyState
{
    public int Value { get; set; }
}

// The steps of your transaction.
public enum MyStep
{
    Start,
    Processing,
    Completed
}

// The response from your handler.
public class MyResponse
{
    public bool IsSuccess { get; set; }
}
```

### 2. Configure Dependency Injection

In your `Program.cs`, configure MediatR, TransactR, and the required components.

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Add MediatR
builder.Services.AddMediatR(cfg => { /* ... */ });

// 2. Add TransactR's behavior and context provider for MediatR
builder.Services.AddTransactRMediatR();

// 3. Register your MementoStore implementation
builder.Services.AddSingleton<IMementoStore<MyState, MyStep>, InMemoryMementoStore<MyState, MyStep>>();

// 4. Register your custom state restorer
builder.Services.AddScoped<IStateRestorer<MyState>, MyStateRestorer>();
```

### 3. Define the Transactional Components

Create the command, the transaction context, and the state restorer.

```csharp
// The command that initiates or continues the transaction.
public class MyCommand : IRequest<MyResponse>, ITransactionalRequest<MyState, MyStep>
{
    public string TransactionId { get; set; }
}

// The context defines the transaction's workflow and outcome logic.
public class MyTransactionContext : TransactionContext<MyTransactionContext, MyState, MyStep, MyResponse>
{
    public override MyStep InitialStep => MyStep.Start;

    // Logic to determine the transaction outcome based on the handler's response.
    public override TransactionOutcome EvaluateResponse(MyResponse response)
    {
        if (!response.IsSuccess)
        {
            return TransactionOutcome.Failed; // This will trigger a rollback.
        }
        
        // You can add logic for multi-step sagas here.
        return TransactionOutcome.Completed;
    }
}

// The logic to restore state in case of an error.
public class MyStateRestorer : IStateRestorer<MyState>
{
    public Task RestoreAsync(MyState state, CancellationToken cancellationToken)
    {
        // Your logic to revert changes in the database or other systems.
        Console.WriteLine($""Restoring state value to: {state.Value}"");
        return Task.CompletedTask;
    }
}
```

### 4. Implement the Command Handler

Access the transaction context and implement your business logic.

```csharp
public class MyCommandHandler : IRequestHandler<MyCommand, MyResponse>
{
    private readonly ITransactionContextProvider<MyTransactionContext> _contextProvider;

    public MyCommandHandler(ITransactionContextProvider<MyTransactionContext> contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public Task<MyResponse> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var context = _contextProvider.Context;
        context.State.Value = 100;

        // If an exception is thrown, a rollback occurs.
        // If IsSuccess is false, a rollback also occurs based on EvaluateResponse.
        return Task.FromResult(new MyResponse { IsSuccess = true });
    }
}
```

### 5. Overriding the Rollback Policy

By default, a failure triggers a rollback to the current step (`RollbackToCurrentStep`). You can override this by implementing `ITransactionalRequestWithPolicy` on your command.

```csharp
public class MyCommandWithPolicy : IRequest<MyResponse>, ITransactionalRequestWithPolicy<MyState, MyStep>
{
    public string TransactionId { get; set; }

    // Specify a different policy, e.g., roll back to the very first step.
    public RollbackPolicy RollbackPolicy => RollbackPolicy.RollbackToBeginning;
}
```

## 🔧 Memento Store Implementations

TransactR is storage-agnostic. You can use an official implementation or create your own by implementing `IMementoStore`.

### TransactR.EntityFramework
[![NuGet](https://img.shields.io/nuget/v/TransactR.EntityFramework)](https://www.nuget.org/packages/TransactR.EntityFramework)
* **Installation:** `dotnet add package TransactR.EntityFramework`
* **DI Integration:** `builder.Services.AddEntityFrameworkMementoStore<ApplicationDbContext, MyState, int>();`

### TransactR.DistributedMemoryCache
[![NuGet](https://img.shields.io/nuget/v/TransactR.DistributedMemoryCache)](https://www.nuget.org/packages/TransactR.DistributedMemoryCache)
* **Installation:** `dotnet add package TransactR.DistributedMemoryCache`
* **DI Integration:** `builder.Services.AddDistributedMemoryCacheMementoStore<MyState, int>();`

### TransactR.MongoDB
[![NuGet](https://img.shields.io/nuget/v/TransactR.MongoDB)](https://www.nuget.org/packages/TransactR.MongoDB)
* **Installation:** `dotnet add package TransactR.MongoDB`
* **DI Integration:** `builder.Services.AddMongoDbMementoStore<MyState, int>(...);`

### TransactR.AzureTableStorage
[![NuGet](https://img.shields.io/nuget/v/TransactR.AzureTableStorage)](https://www.nuget.org/packages/TransactR.AzureTableStorage)
* **Installation:** `dotnet add package TransactR.AzureTableStorage`
* **DI Integration:** `builder.Services.AddAzureTableStorageMementoStore<MyState, int>(...);`

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!
Feel free to check the [issues page](https://github.com/lucafabbri/TransactR/issues).

## 💖 Show Your Support

Please give a ⭐️ if this project helped you!

## 📝 License

This project is licensed under the **MIT License**.