using System;

namespace TransactR;

/// <summary>
/// Contains the context for a transaction, including its ID, current step, and state.
/// </summary>
public interface ITransactionContext<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    string TransactionId { get; }
    TStep Step { get; }
    bool TryIncrementStep();
    bool TryDecrementStep();
    bool TrySetStep(TStep step);

    /// <summary>
    /// Initializes the context for a new transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <returns>The initialized context instance.</returns>
    void Initialize(string transactionId);

    /// <summary>
    /// Evaluates the response from the handler to determine the outcome of the transaction step.
    /// </summary>
    TransactionOutcome EvaluateResponse(object? response = null);
}

