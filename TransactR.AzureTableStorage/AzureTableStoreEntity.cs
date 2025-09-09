using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace TransactR.AzureTableStorage;
/// <summary>
/// Represents a memento entity stored in Azure Table Storage.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public class AzureTableStoreEntity<TStep, TContext> : ITableEntity
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public static AzureTableStoreEntity<TStep, TContext> FromMemento(string transactionId, TContext state)
    {
        return new AzureTableStoreEntity<TStep, TContext>
        {
            PartitionKey = transactionId,
            RowKey = JsonSerializer.Serialize(state.Step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TStep, TContext> ToMemento()
    {
        return new Memento<TStep, TContext>(JsonSerializer.Deserialize<TContext>(State)!);
    }
}
