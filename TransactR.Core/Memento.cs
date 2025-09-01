namespace TransactR
{
    /// <summary>
    /// Represents a stored state (memento) that includes both the state object
    /// and the step at which it was captured.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
    public class Memento<TState, TStep>
        where TState : class, new()
        where TStep : notnull, System.IComparable
    {
        /// <summary>
        /// Gets the step at which the state was saved.
        /// </summary>
        public TStep Step { get; }

        /// <summary>
        /// Gets the saved state object.
        /// </summary>
        public TState State { get; }

        public Memento(TStep step, TState state)
        {
            Step = step;
            State = state ?? throw new System.ArgumentNullException(nameof(state));
        }
    }
}
