using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace TransactR.MongoDB;

public static class TransactRMongoDBServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbMementoStore<TState, TStep>(
        this IServiceCollection services, string connectionString, string databaseName)
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        services.AddScoped<IMementoStore<TState, TStep>, MongoMementoStore<TState, TStep>>();
        return services;
    }
}
