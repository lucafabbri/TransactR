namespace TransactR
{
    /// <summary>
    /// Marker interface for a request that is part of a transaction.
    /// It connects the request to the types of its State and Step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
    public interface ITransactionalRequest<TState, TStep> 
        where TStep : notnull, IComparable
        where TState : class, new()
    {
        /// <summary>
        /// Gets the unique identifier for the transaction.
        /// </summary>
        string TransactionId { get; }
    }
}
