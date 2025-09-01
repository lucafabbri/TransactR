# TransactR

**TransactR** is a lightweight and flexible .NET library for managing transactions and rollbacks based on the Memento pattern. It is designed to be integrated into command processing pipelines, such as those provided by MediatR or Concordia.Core, to save the state of an operation before its execution and automatically restore it in case of an error.

## Key Concepts

* **Memento Pattern**: The core of the library. It captures and stores an object's internal state (the "memento") so that it can be restored later, without violating encapsulation.

* **Transactional Behavior**: A pipeline behavior that intercepts requests, orchestrates the saving of the initial state, and handles rollback logic within a `try/catch` block.

* **State Management**: State persistence is managed through the `IMementoStore` interface, while the restoration logic is delegated to an `IStateRestorer`, allowing for maximum flexibility (in-memory, database, etc.).

* **Interactive Saga**: Supports multi-step processes where the transaction state is preserved across multiple calls, enabling the continuation of complex workflows.

* **Disaster Recovery**: Offers configurable rollback policies (`RollbackToCurrentStep`, `RollbackToBeginning`, `DeleteTransactionState`) to handle unexpected exceptions.

## Installation

The library is split into packages to maintain modularity. Install the Core package and your desired integration package.

```shell
dotnet add package TransactR.Core
dotnet add package TransactR.MediatR  # or TransactR.Concordia
```

## Usage Example (with MediatR)

### 1. Configuration (Dependency Injection)

```csharp
// Program.cs
builder.Services.AddMediatR(cfg => { /* ... */ });

// Adds the behavior and the context provider
builder.Services.AddTransactRMediatR(); 

// Register your implementations
builder.Services.AddSingleton<IMementoStore<MyState, MyStep>, InMemoryMementoStore<MyState, MyStep>>();
builder.Services.AddScoped<IStateRestorer<MyState>, MyStateRestorer>();
```

### 2. Component Definitions

```csharp
// The command that triggers the transaction
public class MyCommand : IRequest, ITransactionalRequest<MyState, MyStep>
{
    public string TransactionId { get; set; }
}

// The context that manages the transaction's state and logic
public class MyContext : TransactionContext<MyContext, MyState, MyStep>
{
    public override MyStep InitialStep => MyStep.Start;

    public override TransactionOutcome EvaluateResponse<TResponse>(TResponse response)
    {
        // Logic to decide if the transaction is completed, in progress, or failed
        return TransactionOutcome.Completed;
    }
}

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

```csharp
public class MyCommandHandler : IRequestHandler<MyCommand>
{
    private readonly ITransactionContextProvider<MyContext> _contextProvider;

    public MyCommandHandler(ITransactionContextProvider<MyContext> contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public Task Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Access the context automatically created by the behavior
        var context = _contextProvider.Context;
        
        // Execute your business logic
        context.State.Value = 100;

        // If an exception is thrown here, IStateRestorer.RestoreAsync will be invoked.

        return Task.CompletedTask;
    }
}
```
