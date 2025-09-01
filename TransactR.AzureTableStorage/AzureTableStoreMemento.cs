using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace TransactR.AzureTableStorage;

/// <summary>
/// An Azure Table Storage implementation of <see cref="IMementoStore{TState, TStep}"/>.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public class AzureTableStoreMemento<TState, TStep> : IMementoStore<TState, TStep>
    where TStep : notnull, IComparable
    where TState : class, new()
{
    private readonly TableClient _tableClient;
    private readonly string _tableName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTableStoreMemento{TState, TStep}"/> class.
    /// </summary>
    /// <param name="tableClient">The Azure Table Storage client.</param>
    /// <param name="tableName">The name of the table to use.</param>
    public AzureTableStoreMemento(TableClient tableClient, string tableName = "mementos")
    {
        _tableClient = tableClient;
        _tableName = tableName;
        _tableClient.CreateIfNotExists();
    }

    /// <inheritdoc />
    public async Task SaveAsync(string transactionId, TStep step, TState state, CancellationToken cancellationToken = default)
    {
        var entity = AzureTableStoreEntity<TState>.FromMemento(transactionId, step, state);
        await _tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TState?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tableClient.GetEntityAsync<AzureTableStoreEntity<TState>>(transactionId, System.Text.Json.JsonSerializer.Serialize(step), cancellationToken: cancellationToken);
            return entity.Value.ToMemento<TStep>().State;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        await _tableClient.DeleteEntityAsync(transactionId, System.Text.Json.JsonSerializer.Serialize(step), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entities = _tableClient.Query<AzureTableStoreEntity<TState>>(e => e.PartitionKey == transactionId)
                                  .OrderBy(e => e.RowKey)
                                  .Take(1);

        var firstEntity = entities.FirstOrDefault();
        if (firstEntity == null)
            return default;
        return firstEntity.ToMemento<TStep>().Step;
    }

    /// <inheritdoc />
    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entities = _tableClient.QueryAsync<AzureTableStoreEntity<TState>>(e => e.PartitionKey == transactionId, cancellationToken: cancellationToken);

        await foreach (var entity in entities)
        {
            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Memento<TState, TStep>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entities = _tableClient.Query<AzureTableStoreEntity<TState>>(e => e.PartitionKey == transactionId)
                                  .OrderByDescending(e => e.RowKey)
                                  .Take(1);

        var latestEntity = entities.FirstOrDefault();
        return latestEntity?.ToMemento<TStep>();
    }
}
