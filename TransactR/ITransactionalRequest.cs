namespace TransactR;

/// <summary>
/// Marker interface for a request that is part of a transaction.
/// It connects the request to the types of its State and Step.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
public interface ITransactionalRequest<TState>
    where TState : class, IState, new()
{
    /// <summary>
    /// Gets the unique identifier for the transaction.
    /// </summary>
    string TransactionId { get; }

    /// <summary>
    /// Gets the step size used for incrementing or decrementing values in a sequence.
    /// </summary>
    IComparable Step { get; }

    /// <summary>
    /// Gets the rollback policy to apply in case of an unhandled exception.
    /// </summary>
    RollbackPolicy RollbackPolicy { get; }
}

public abstract class TransactionalRequest<TState>(string transactionId, IComparable step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : ITransactionalRequest<TState>
    where TState : class, IState, new()
{
    public string TransactionId { get; } = transactionId;
    public IComparable Step { get; } = step;
    public RollbackPolicy RollbackPolicy { get; } = rollbackPolicy;
}
