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
public class InMemoryMementoStore<TState> : IMementoStore<TState>
    where TState : class, IState, new()
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<IComparable,TState>> _store = new();

    public Task SaveAsync(string transactionId, TState state, CancellationToken cancellationToken = default)
    {
        var transactionMementos = _store.GetOrAdd(transactionId, _ => new ConcurrentDictionary<IComparable, TState>());
        transactionMementos[state.Step] = state;
        return Task.CompletedTask;
    }

    public Task<TState?> RetrieveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) &&
            transactionMementos.TryGetValue(step, out var state))
        {
            return Task.FromResult<TState?>(state);
        }
        return Task.FromResult<TState?>(null);
    }

    public Task RemoveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos))
        {
            transactionMementos.TryRemove(step, out _);
        }
        return Task.CompletedTask;
    }

    public Task<IComparable?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) && !transactionMementos.IsEmpty)
        {
            var firstStep = transactionMementos.Keys.Min();
            return Task.FromResult(firstStep);
        }
        return Task.FromResult<IComparable?>(default);
    }

    public Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(transactionId, out _);
        return Task.CompletedTask;
    }

    public Task<Memento<TState>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(transactionId, out var transactionMementos) && !transactionMementos.IsEmpty)
        {
            var latestStep = transactionMementos.Keys.Max();
            if (transactionMementos.TryGetValue(latestStep, out var state))
            {
                return Task.FromResult<Memento<TState>?>(new Memento<TState>(state));
            }
        }
        return Task.FromResult<Memento<TState>?>(null);
    }
}

