using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace TransactR.AzureTableStorage;
/// <summary>
/// Represents a memento entity stored in Azure Table Storage.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public class AzureTableStoreEntity<TState> : ITableEntity
        where TState : class, new()
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }

    public string State { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public static AzureTableStoreEntity<TState> FromMemento<TStep>(string transactionId, TStep step, TState state)
        where TStep : notnull, IComparable
    {
        return new AzureTableStoreEntity<TState>
        {
            PartitionKey = transactionId,
            // The RowKey must be a string. We will serialize the step to ensure proper sorting and storage.
            RowKey = JsonSerializer.Serialize(step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TState, TStep> ToMemento<TStep>()
        where TStep : notnull, IComparable
    {
        return new Memento<TState, TStep>(
            JsonSerializer.Deserialize<TStep>(RowKey)!,
            JsonSerializer.Deserialize<TState>(State)!
        );
    }
}
