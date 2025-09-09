using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace TransactR.AzureTableStorage;

/// <summary>
/// An Azure Table Storage implementation of <see cref="IMementoStore{TStep, TContext}"/>.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public class AzureTableStoreMemento<TStep, TContext> : IMementoStore<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
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
    public async Task SaveAsync(string transactionId, TContext state, CancellationToken cancellationToken = default)
    {
        var entity = AzureTableStoreEntity<TStep, TContext>.FromMemento(transactionId, state);
        await _tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TContext?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tableClient.GetEntityAsync<AzureTableStoreEntity<TStep, TContext>>(transactionId, System.Text.Json.JsonSerializer.Serialize(step), cancellationToken: cancellationToken);
            return entity.Value.ToMemento().State;
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
        var entities = _tableClient.Query<AzureTableStoreEntity<TStep, TContext>>(e => e.PartitionKey == transactionId)
                                  .OrderBy(e => e.RowKey)
                                  .Take(1);

        var firstEntity = entities.FirstOrDefault();
        if (firstEntity == null)
            return default;
        return firstEntity.ToMemento().State.Step;
    }

    /// <inheritdoc />
    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entities = _tableClient.QueryAsync<AzureTableStoreEntity<TStep, TContext>>(e => e.PartitionKey == transactionId, cancellationToken: cancellationToken);

        await foreach (var entity in entities)
        {
            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Memento<TStep, TContext>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entities = _tableClient.Query<AzureTableStoreEntity<TStep, TContext>>(e => e.PartitionKey == transactionId)
                                  .OrderByDescending(e => e.RowKey)
                                  .Take(1);

        var latestEntity = entities.FirstOrDefault();
        return latestEntity?.ToMemento();
    }
}
