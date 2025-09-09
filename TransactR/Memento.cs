namespace TransactR;

/// <summary>
/// Represents a stored state (memento) that includes both the state object
/// and the step at which it was captured.
/// </summary>
/// <typeparam name="TContext">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public class Memento<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{

    /// <summary>
    /// Gets the saved state object.
    /// </summary>
    public TContext State { get; }

    public Memento(TContext state)
    {
        State = state;
    }
}
