using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace TransactR;

/// <summary>
/// An in-memory implementation of IMementoStore for testing and simple scenarios.
/// This implementation is thread-safe.
/// </summary>
public class InMemoryMementoStore<TStep, TContext> : IMementoStore<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<TStep, TContext>> _store = new();

    public Task SaveAsync(string transactionId, TContext state, CancellationToken cancellationToken = default)
    {
        var transactionMementos = _store.GetOrAdd(transactionId, _ => new ConcurrentDictionary<TStep, TContext>());
        transactionMementos[state.Step] = state;
        return Task.CompletedTask;
    }

    public Task<TContext?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) &&
            transactionMementos.TryGetValue(step, out var state))
        {
            return Task.FromResult<TContext?>(state);
        }
        return Task.FromResult<TContext?>(null);
    }

    public Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos))
        {
            transactionMementos.TryRemove(step, out _);
        }
        return Task.CompletedTask;
    }

    public Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) && !transactionMementos.IsEmpty)
        {
            var firstStep = transactionMementos.Keys.Min();
            return Task.FromResult(firstStep);
        }
        return Task.FromResult<TStep?>(default);
    }

    public Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(transactionId, out _);
        return Task.CompletedTask;
    }

    public Task<Memento<TStep, TContext>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) && !transactionMementos.IsEmpty)
        {
            var latestStep = transactionMementos.Keys.Max();
            if (transactionMementos.TryGetValue(latestStep, out var state))
            {
                return Task.FromResult<Memento<TStep, TContext>?>(new Memento<TStep, TContext>(state));
            }
        }
        return Task.FromResult<Memento<TStep, TContext>?>(null);
    }
}

