using MongoDB.Driver;
using System.Text.Json;

namespace TransactR.MongoDB;

public class MongoMementoStore<TState, TStep> : IMementoStore<TState, TStep>
    where TState : class, new()
    where TStep : notnull, IComparable
{
    private readonly IMongoCollection<MongoMementoEntity<TState>> _collection;

    public MongoMementoStore(IMongoDatabase database)
    {
        _collection = database.GetCollection<MongoMementoEntity<TState>>("Mementos");
    }

    public async Task SaveAsync(string transactionId, TStep step, TState state, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(step));

        var document = MongoMementoEntity<TState>.FromMemento(transactionId, step, state);

        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<TState?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(step));

        var entity = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return null;
        return JsonSerializer.Deserialize<TState>(entity.State);
    }

    public async Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(step));

        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId);
        var entity = await _collection.Find(filter).SortBy(x => x.Step).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return default;
        return JsonSerializer.Deserialize<TStep>(entity.Step);
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId);
        await _collection.DeleteManyAsync(filter, cancellationToken);
    }

    public async Task<Memento<TState, TStep>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TState>>.Filter.Eq(x => x.TransactionId, transactionId);
        var entity = await _collection.Find(filter).SortByDescending(x => x.Step).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return null;
        return entity.ToMemento<TStep>();
    }
}
