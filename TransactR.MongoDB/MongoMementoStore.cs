using MongoDB.Driver;
using System.Text.Json;

namespace TransactR.MongoDB;

public class MongoMementoStore<TStep, TContext> : IMementoStore<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    private readonly IMongoCollection<MongoMementoEntity<TStep, TContext>> _collection;

    public MongoMementoStore(IMongoDatabase database)
    {
        _collection = database.GetCollection<MongoMementoEntity<TStep, TContext>>("Mementos");
    }

    public async Task SaveAsync(string transactionId, TContext state, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(state.Step));

        var document = MongoMementoEntity<TStep, TContext>.FromMemento(transactionId, state);

        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<TContext?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(step));

        var entity = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return null;
        return JsonSerializer.Deserialize<TContext>(entity.State);
    }

    public async Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId) &
                     Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.Step, JsonSerializer.Serialize(step));

        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId);
        var entity = await _collection.Find(filter).SortBy(x => x.Step).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return default;
        return JsonSerializer.Deserialize<TContext>(entity.State).Step;
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId);
        await _collection.DeleteManyAsync(filter, cancellationToken);
    }

    public async Task<Memento<TStep, TContext>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<MongoMementoEntity<TStep, TContext>>.Filter.Eq(x => x.TransactionId, transactionId);
        var entity = await _collection.Find(filter).SortByDescending(x => x.Step).FirstOrDefaultAsync(cancellationToken);
        if (entity is null) return null;
        return entity.ToMemento();
    }
}
