using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;

namespace TransactR.MongoDB;

public class MongoMementoEntity<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public string Step { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public static MongoMementoEntity<TStep, TContext> FromMemento(string transactionId, TContext state)
    {
        return new MongoMementoEntity<TStep, TContext>
        {
            TransactionId = transactionId,
            Step = JsonSerializer.Serialize(state.Step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TStep, TContext> ToMemento()
    {
        return new Memento<TStep, TContext>(
            JsonSerializer.Deserialize<TContext>(State)!
        );
    }
}