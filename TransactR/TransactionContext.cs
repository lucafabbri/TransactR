using System;

namespace TransactR
{
    /// <summary>
    /// Provides the base implementation for a transaction context.
    /// Consumers must inherit from this class to provide a concrete evaluation logic.
    /// </summary>
    public abstract class TransactionContext<TContext, TState, TStep, TResponse> : ITransactionContext<TContext, TState, TStep, TResponse>
        where TContext : TransactionContext<TContext, TState, TStep, TResponse>
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        public string TransactionId { get; protected set; } = string.Empty;
        public abstract TStep InitialStep { get; }
        public TStep Step { get; protected set; }
        public TState State { get; protected set; } = new();

        /// <summary>
        /// When implemented in a derived class, evaluates the response from the handler
        /// to determine the outcome of the transaction step.
        /// </summary>
        public abstract TransactionOutcome EvaluateResponse(TResponse response);

        public TContext Initialize(string transactionId)
        {
            TransactionId = transactionId;
            Step = InitialStep;
            State = new TState();
            return (TContext)this;
        }

        public TContext Hydrate(string transactionId, TStep step, TState state)
        {
            TransactionId = transactionId;
            Step = step;
            State = state;
            return (TContext)this;
        }
    }
}

