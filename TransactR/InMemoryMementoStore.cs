using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace TransactR
{
    /// <summary>
    /// An in-memory implementation of IMementoStore for testing and simple scenarios.
    /// This implementation is thread-safe.
    /// </summary>
    public class InMemoryMementoStore<TState, TStep> : IMementoStore<TState, TStep>
        where TStep : notnull, IComparable
        where TState : class, new()
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<TStep, TState>> _store = new();

        public Task SaveAsync(string transactionId, TStep step, TState state, CancellationToken cancellationToken = default)
        {
            var transactionMementos = _store.GetOrAdd(transactionId, _ => new ConcurrentDictionary<TStep, TState>());
            transactionMementos[step] = state;
            return Task.CompletedTask;
        }

        public Task<TState?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(transactionId, out var transactionMementos) &&
                transactionMementos.TryGetValue(step, out var state))
            {
                return Task.FromResult<TState?>(state);
            }
            return Task.FromResult<TState?>(null);
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
                return Task.FromResult<TStep?>(firstStep);
            }
            return Task.FromResult<TStep?>(default);
        }

        public Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            _store.TryRemove(transactionId, out _);
            return Task.CompletedTask;
        }

        public Task<Memento<TState, TStep>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(transactionId, out var transactionMementos) && !transactionMementos.IsEmpty)
            {
                var latestStep = transactionMementos.Keys.Max();
                if (transactionMementos.TryGetValue(latestStep, out var state))
                {
                    return Task.FromResult<Memento<TState, TStep>?>(new Memento<TState, TStep>(latestStep, state));
                }
            }
            return Task.FromResult<Memento<TState, TStep>?>(null);
        }
    }
}

