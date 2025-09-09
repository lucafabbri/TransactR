namespace TransactR;

/// <summary>
/// A default, concrete implementation of ITransactionContextProvider.
/// </summary>
/// <typeparam name="TContext">The type of the transaction context.</typeparam>
public class TransactionContextProvider<TStep, TContext> : ITransactionContextProvider<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    /// <summary>
    /// Gets or sets the current transaction context.
    /// </summary>
    public TContext Context { get; set; }
}

