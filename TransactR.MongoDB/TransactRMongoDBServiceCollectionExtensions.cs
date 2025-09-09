using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace TransactR.MongoDB;

public static class TransactRMongoDBServiceCollectionExtensions
{
    public static ITransactorBuilder<TStep, TContext> PersistedInMongo<TStep, TContext>(
        this ITransactorBuilder<TStep, TContext> transactorBuilder, string connectionString, string databaseName)
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        transactorBuilder.Options.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        transactorBuilder.Options.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        transactorBuilder.Options.Services.AddScoped<IMementoStore<TStep, TContext>, MongoMementoStore<TStep, TContext>>();
        return transactorBuilder;
    }
}
