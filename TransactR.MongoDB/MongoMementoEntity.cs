using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;

namespace TransactR.MongoDB;

public class MongoMementoEntity<TState>
        where TState : class, new()
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public string Step { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public static MongoMementoEntity<TState> FromMemento<TStep>(string transactionId, TStep step, TState state)
        where TStep : notnull, IComparable
    {
        return new MongoMementoEntity<TState>
        {
            TransactionId = transactionId,
            Step = JsonSerializer.Serialize(step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TState, TStep> ToMemento<TStep>()
        where TStep : notnull, IComparable
    {
        return new Memento<TState, TStep>(
            JsonSerializer.Deserialize<TStep>(Step)!,
            JsonSerializer.Deserialize<TState>(State)!
        );
    }
}