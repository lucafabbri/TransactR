namespace TransactR;

/// <summary>
/// Represents a stored state (memento) that includes both the state object
/// and the step at which it was captured.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public class Memento<TState>
    where TState : class, IState, new()
{

    /// <summary>
    /// Gets the saved state object.
    /// </summary>
    public TState State { get; }

    public Memento(TState state)
    {
        State = state;
    }
}
