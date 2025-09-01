# TransactR.AzureTableStorage

`TransactR.AzureTableStorage` is a library that provides an `IMementoStore` implementation using Azure Table Storage. It's a highly performant and cost-effective solution for persisting mementos in large-scale applications.

## Installation

First, install the NuGet package into your project.

```
dotnet add package TransactR.AzureTableStorage
```

## Configuration

To use the `AzureTableStorageMementoStore`, you need to configure it in your `Program.cs` or `Startup.cs` file.

You need to provide the Azure Storage account connection string.

```csharp
// Program.cs
builder.Services.AddAzureTableStorageMementoStore<IlMioStato, int>(
    builder.Configuration.GetConnectionString("AzureStorageConnectionString"),
    tableName: "mementos"
);
```


> **Note:** The `tableName` parameter is optional and defaults to `"mementos"`.

## Usage Example

After configuring the service, you can inject `IMementoStore<IlMioStato, int>` into your services or controllers to save and retrieve mementos.

```csharp
public class IlMioServizio
{
    private readonly IMementoStore<IlMioStato, int> _mementoStore;

    public IlMioServizio(IMementoStore<IlMioStato, int> mementoStore)
    {
        _mementoStore = mementoStore;
    }

    public async Task EseguiOperazioneAsync(string transactionId, IlMioStato stato)
    {
        // Save a new memento
        await _mementoStore.SaveAsync(transactionId, 1, stato);

        // Retrieve a memento
        var mementoRecuperato = await _mementoStore.RetrieveAsync(transactionId, 1);
        
        // Remove a memento
        await _mementoStore.RemoveAsync(transactionId, 1);
    }
}
