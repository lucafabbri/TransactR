namespace TransactR
{
    /// <summary>
    /// Extends ITransactionalRequest to include a specific rollback policy for disaster recovery.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
    public interface ITransactionalRequestWithPolicy<TState, TStep> : ITransactionalRequest<TState, TStep>
        where TStep : notnull, IComparable
        where TState : class, new()
    {
        /// <summary>
        /// Gets the rollback policy to apply in case of an unhandled exception.
        /// </summary>
        RollbackPolicy RollbackPolicy { get; }
    }
}
