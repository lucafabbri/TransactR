using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactR.Behaviors;

namespace TransactR;


public static class TransactRDependencyInjection
{

    public static ITransactorBuilder AddTransactR(this IServiceCollection services)
    {
        var options = new TransactorBuilderOptions(services);
        return new TransactorBuilder(options);
    }
}

public interface ITransactorBuilder
{
    TransactorBuilderOptions Options { get; }
    ITransactorBuilder<TStep, TContext> HasState<TStep, TContext>()
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new();
}

internal class TransactorBuilder : ITransactorBuilder
{
    public TransactorBuilder(TransactorBuilderOptions options)
    {
        Options = options;
    }
    public TransactorBuilderOptions Options { get; }

    public ITransactorBuilder<TStep, TContext> HasState<TStep, TContext>()
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        Options.Services.AddScoped<ITransactionContextProvider<TStep, TContext>, TransactionContextProvider<TStep, TContext>>();

        return new TransactorBuilder<TStep, TContext>(Options);
    }
}

public interface ITransactorBuilder<TStep, TContext> : ITransactorBuilder
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    ITransactionContextBuilder<TStep, TContext> PersistedInMemory();
    ITransactionContextBuilder<TStep, TContext> UseMementoStore<TMementoStore>()
        where TMementoStore : class, IMementoStore<TStep, TContext>;
    ITransactionContextBuilder<TStep, TContext> RestoredBy<TStateRestorer>()
        where TStateRestorer : class, IStateRestorer<TStep, TContext>;
}

internal class TransactorBuilder<TStep, TContext> : TransactorBuilder, ITransactorBuilder<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public TransactorBuilder(TransactorBuilderOptions options) : base(options)
    {
    }

    public ITransactionContextBuilder<TStep, TContext> PersistedInMemory()
    {
        return UseMementoStore<InMemoryMementoStore<TStep, TContext>>();
    }

    public ITransactionContextBuilder<TStep, TContext> UseMementoStore<TMementoStore>()
        where TMementoStore : class, IMementoStore<TStep, TContext>
    {
        Options.Services.AddSingleton<IMementoStore<TStep, TContext>, TMementoStore>();
        if (Options.TransactionContextBuilderFactory == null)
        {
            throw new InvalidOperationException("TransactionContextBuilderFactory is not set. Please set it before using this method.");
        }
        return Options.TransactionContextBuilderFactory.Create<TStep, TContext>(Options);
    }

    public ITransactionContextBuilder<TStep, TContext> RestoredBy<TStateRestorer>()
        where TStateRestorer : class, IStateRestorer<TStep, TContext>
    {
        Options.Services.AddScoped<IStateRestorer<TStep, TContext>, TStateRestorer>();

        if (Options.TransactionContextBuilderFactory == null)
        {
            throw new InvalidOperationException("TransactionContextBuilderFactory is not set. Please set it before using this method.");
        }

        return Options.TransactionContextBuilderFactory.Create<TStep, TContext>(Options);
    }
}

public interface ITransactionContextBuilder<TStep, TContext> : ITransactorBuilder<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    ITransactionContextBuilder<TStep, TContext> Surround<TRequest>()
        where TRequest : ITransactionalRequest<TStep, TContext>;

    ITransactionContextBuilder<TStep, TContext> Surround(Type requestType);
}
