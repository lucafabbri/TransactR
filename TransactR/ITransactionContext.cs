using System;

namespace TransactR;

/// <summary>
/// Contains the context for a transaction, including its ID, current step, and state.
/// </summary>
public interface ITransactionContext<TContext, TState>
    where TContext : ITransactionContext<TContext, TState>
    where TState : class, IState, new()
{
    string TransactionId { get; }
    TState State { get; }

    /// <summary>
    /// Initializes the context for a new transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <returns>The initialized context instance.</returns>
    TContext Initialize(string transactionId);

    /// <summary>
    /// Hydrates the context with data from an existing transaction.
    /// </summary>
    /// <param name="transactionId">The transaction's unique identifier.</param>
    /// <param name="step">The current step of the transaction.</param>
    /// <param name="state">The current state of the transaction.</param>
    /// <returns>The hydrated context instance.</returns>
    TContext Hydrate(string transactionId, TState state);

    /// <summary>
    /// Evaluates the response from the handler to determine the outcome of the transaction step.
    /// </summary>
    TransactionOutcome EvaluateResponse(object? response = null);
}

