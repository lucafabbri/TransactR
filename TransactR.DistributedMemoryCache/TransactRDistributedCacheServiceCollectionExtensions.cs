using Microsoft.Extensions.DependencyInjection;

namespace TransactR.DistributedMemoryCache;

public static class TransactRDistributedCacheServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedCacheMementoStore<TState, TStep>(this IServiceCollection services)
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        services.AddSingleton<IMementoStore<TState, TStep>, DistributedMemoryCacheMementoStore<TState, TStep>>();
        return services;
    }
}
