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
    ITransactorBuilder<TState> HasState<TState>()
        where TState : class, IState, new();
}

internal class TransactorBuilder : ITransactorBuilder
{
    public TransactorBuilder(TransactorBuilderOptions options)
    {
        Options = options;
    }
    public TransactorBuilderOptions Options { get; }

    public ITransactorBuilder<TState> HasState<TState>()
        where TState : class, IState, new()
    {
        return new TransactorBuilder<TState>(Options);
    }
}

public interface ITransactorBuilder<TState> : ITransactorBuilder
    where TState : class, IState, new()
{
    ITransactionContextBuilder<TState, TTransactionContext> UseContext<TTransactionContext>()
        where TTransactionContext : class, ITransactionContext<TState>, new();
    ITransactorBuilder<TState> PersistedInMemory();
    ITransactorBuilder<TState> UseMementoStore<TMementoStore>()
        where TMementoStore : class, IMementoStore<TState>;
    ITransactorBuilder<TState> RestoredBy<TStateRestorer>()
        where TStateRestorer : class, IStateRestorer<TState>;
}

internal class TransactorBuilder<TState> : TransactorBuilder, ITransactorBuilder<TState>
    where TState : class, IState, new()
{
    public TransactorBuilder(TransactorBuilderOptions options) : base(options)
    {
    }

    public ITransactionContextBuilder<TState, TTransactionContext> UseContext<TTransactionContext>()
        where TTransactionContext : class, ITransactionContext<TState>, new()
    {
        Options.Services.AddScoped<ITransactionContextProvider<TTransactionContext>, TransactionContextProvider<TTransactionContext>>();
        Options.Services.AddScoped<ITransactionContext<TState>, TTransactionContext>();

        if (Options.TransactionContextBuilderFactory != null)
        {
            return Options.TransactionContextBuilderFactory.Create<TState, TTransactionContext>(Options);
        }

        throw new InvalidOperationException("Transaction context builder factory not registered. Did you forget to call an integration-specific method like .OnConcordia()? or like .OnMediatR()?");
    }

    public ITransactorBuilder<TState> PersistedInMemory()
    {
        return UseMementoStore<InMemoryMementoStore<TState>>();
    }

    public ITransactorBuilder<TState> UseMementoStore<TMementoStore>()
        where TMementoStore : class, IMementoStore<TState>
    {
        Options.Services.AddSingleton<IMementoStore<TState>, TMementoStore>();
        return this;
    }

    public ITransactorBuilder<TState> RestoredBy<TStateRestorer>()
        where TStateRestorer : class, IStateRestorer<TState>
    {
        Options.Services.AddScoped<IStateRestorer<TState>, TStateRestorer>();
        return this;
    }
}

public interface ITransactionContextBuilder<TState, TTransactionContext> : ITransactorBuilder<TState>
    where TState : class, IState, new()
    where TTransactionContext : class, ITransactionContext<TState>, new()
{
    ITransactionContextBuilder<TState, TTransactionContext> Surround<TRequest>()
        where TRequest : ITransactionalRequest<TState>;

    ITransactionContextBuilder<TState, TTransactionContext> Surround(Type requestType);
}
