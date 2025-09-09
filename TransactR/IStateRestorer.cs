using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactR;


/// <summary>
/// Defines the contract for a component capable of restoring
/// the system's state to a previous version (memento).
/// </summary>
/// <typeparam name="TState">The type of state to restore.</typeparam>
public interface IStateRestorer<in TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    /// <summary>
    /// Restores the system's state using the provided memento.
    /// </summary>
    /// <param name="state">The state to restore.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RestoreAsync(TContext state, CancellationToken cancellationToken = default);
}
