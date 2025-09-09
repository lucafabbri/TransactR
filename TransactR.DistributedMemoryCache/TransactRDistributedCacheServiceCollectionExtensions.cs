using Microsoft.Extensions.DependencyInjection;

namespace TransactR.DistributedMemoryCache;

public static class TransactRDistributedCacheServiceCollectionExtensions
{
    public static ITransactorBuilder<TStep, TContext> Cached<TStep, TContext>(
        this ITransactorBuilder<TStep, TContext> transactorBuilder)
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        transactorBuilder.Options.Services.AddSingleton<IMementoStore<TStep, TContext>, DistributedMemoryCacheMementoStore<TStep, TContext>>();
        return transactorBuilder;
    }
}
