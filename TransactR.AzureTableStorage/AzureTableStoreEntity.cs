using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace TransactR.AzureTableStorage;
/// <summary>
/// Represents a memento entity stored in Azure Table Storage.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public class AzureTableStoreEntity<TState> : ITableEntity
        where TState : class, IState, new()
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public static AzureTableStoreEntity<TState> FromMemento(string transactionId, TState state)
    {
        return new AzureTableStoreEntity<TState>
        {
            PartitionKey = transactionId,
            RowKey = JsonSerializer.Serialize(state.Step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TState> ToMemento()
    {
        return new Memento<TState>(JsonSerializer.Deserialize<TState>(State)!);
    }
}
