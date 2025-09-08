using System;
using System.Threading;
using System.Threading.Tasks;

namespace TransactR;

/// <summary>
/// Manages the persistence of mementos.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public interface IMementoStore<TState>
    where TState : class, IState, new()
{
    /// <summary>
    /// Saves a state for a specific transaction and step.
    /// </summary>
    Task SaveAsync(string transactionId, TState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a state for a specific transaction and step.
    /// </summary>
    Task<TState?> RetrieveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a state for a specific transaction and step.
    /// </summary>
    Task RemoveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the first (e.g., lowest) step recorded for a given transaction.
    /// </summary>
    Task<IComparable?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all states associated with a given transaction.
    /// </summary>
    Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest saved memento (state and step) for a given transaction.
    /// </summary>
    Task<Memento<TState>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default);
}

