using System;

namespace TransactR;

/// <summary>
/// Provides the base implementation for a transaction context.
/// Consumers must inherit from this class to provide a concrete evaluation logic.
/// </summary>
public abstract class TransactionContext<TState> : ITransactionContext<TState>
    where TState : class, IState, new()
{
    public string TransactionId { get; protected set; } = string.Empty;
    public abstract IComparable InitialStep { get; }
    public TState State { get; protected set; } = new();

    /// <summary>
    /// When implemented in a derived class, evaluates the response from the handler
    /// to determine the outcome of the transaction step.
    /// </summary>
    public abstract TransactionOutcome EvaluateResponse(object? response = null);

    public void Initialize(string transactionId)
    {
        TransactionId = transactionId;
        State = new TState();
    }

    public void Hydrate(string transactionId, TState state)
    {
        TransactionId = transactionId;
        State = state;
    }
}

