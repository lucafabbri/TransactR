using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;

namespace TransactR.MongoDB;

public class MongoMementoEntity<TState>
        where TState : class, IState, new()
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public string Step { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public static MongoMementoEntity<TState> FromMemento(string transactionId, TState state)
    {
        return new MongoMementoEntity<TState>
        {
            TransactionId = transactionId,
            Step = JsonSerializer.Serialize(state.Step),
            State = JsonSerializer.Serialize(state)
        };
    }

    public Memento<TState> ToMemento()
    {
        return new Memento<TState>(
            JsonSerializer.Deserialize<TState>(State)!
        );
    }
}