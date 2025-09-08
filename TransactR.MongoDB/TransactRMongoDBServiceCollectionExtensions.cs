using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace TransactR.MongoDB;

public static class TransactRMongoDBServiceCollectionExtensions
{
    public static ITransactorBuilder<TState> PersistedInMongo<TState>(
        this ITransactorBuilder<TState> transactorBuilder, string connectionString, string databaseName)
        where TState : class, IState, new()
    {
        transactorBuilder.Options.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        transactorBuilder.Options.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        transactorBuilder.Options.Services.AddScoped<IMementoStore<TState>, MongoMementoStore<TState>>();
        return transactorBuilder;
    }
}
