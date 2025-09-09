namespace TransactR;

/// <summary>
/// Marker interface for a request that is part of a transaction.
/// It connects the request to the types of its State and Step.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public interface ITransactionalRequest<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    /// <summary>
    /// Gets the unique identifier for the transaction.
    /// </summary>
    string TransactionId { get; }

    /// <summary>
    /// Gets the step size used for incrementing or decrementing values in a sequence.
    /// </summary>
    TStep Step { get; }

    /// <summary>
    /// Gets the rollback policy to apply in case of an unhandled exception.
    /// </summary>
    RollbackPolicy RollbackPolicy { get; }
}

public abstract class TransactionalRequest<TStep, TContext>(
    string transactionId,
    TStep step,
    RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : ITransactionalRequest<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public string TransactionId { get; } = transactionId;
    public TStep Step { get; } = step;
    public RollbackPolicy RollbackPolicy { get; } = rollbackPolicy;
}
