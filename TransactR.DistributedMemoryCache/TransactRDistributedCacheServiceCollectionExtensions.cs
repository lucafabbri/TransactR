using Microsoft.Extensions.DependencyInjection;

namespace TransactR.DistributedMemoryCache;

public static class TransactRDistributedCacheServiceCollectionExtensions
{
    public static ITransactorBuilder<TState> Cached<TState>(
        this ITransactorBuilder<TState> transactorBuilder)
        where TState : class, IState, new()
    {
        transactorBuilder.Options.Services.AddSingleton<IMementoStore<TState>, DistributedMemoryCacheMementoStore<TState>>();
        return transactorBuilder;
    }
}
