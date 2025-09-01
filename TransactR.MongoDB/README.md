# TransactR.MongoDB

`TransactR.MongoDB` is an implementation of `IMementoStore` that uses **MongoDB** as its persistence layer. It's ideal for applications that require a flexible, schemaless data model to store their mementos.

## Installation

First, install the NuGet package into your project.

```
dotnet add package TransactR.MongoDB
```

## Configuration

To use the MongoDB memento store, you need to configure the MongoDB client and then add the `TransactR` service.

```csharp
// Program.cs
builder.Services.AddMongoDbMementoStore<IlMioStato, int>(
    connectionString: "mongodb://localhost:27017",
    databaseName: "TransactRDB");
```

## Usage

After configuration, you can inject `IMementoStore<IlMioStato, int>` into your services or controllers to save and retrieve mementos.

```csharp
public class MyService
{
    private readonly IMementoStore<IlMioStato, int> _mementoStore;

    public MyService(IMementoStore<IlMioStato, int> mementoStore)
    {
        _mementoStore = mementoStore;
    }

    public async Task PerformOperationAsync(string transactionId, IlMioStato state)
    {
        // Save a new memento with step 1
        await _mementoStore.SaveAsync(transactionId, 1, state);
        
        // Retrieve the memento
        var recoveredMemento = await _mementoStore.RetrieveAsync(transactionId, 1);
    }
}
```

`IlMioStato` is a class that can be serialized to JSON.
