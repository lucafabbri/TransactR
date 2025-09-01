# TransactR.DistributedMemoryCache

`TransactR.DistributedMemoryCache` provides an implementation of `IMementoStore` using the `IDistributedCache` service, ideal for high-performance and scalable applications that leverage distributed caching.

## Installation

First, install the NuGet package into your project.

```
dotnet add package TransactR.DistributedMemoryCache

```

## Configuration

To use the distributed cache as your memento store, you need to configure `IDistributedCache` and then add the `TransactR` service. This example assumes you are using **Redis**.

```
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "your_redis_connection_string";
    options.InstanceName = "TransactR_";
});

builder.Services.AddDistributedCacheMementoStore<IlMioStato, int>();

```

## Usage

After configuration, you can inject `IMementoStore<IlMioStato, int>` into your services or controllers to save and retrieve mementos.

```
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
