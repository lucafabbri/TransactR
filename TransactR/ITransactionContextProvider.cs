namespace TransactR;

/// <summary>
/// Defines a contract for a provider that holds the current transaction context within a specific scope.
/// </summary>
/// <typeparam name="TContext">The type of the transaction context.</typeparam>
public interface ITransactionContextProvider<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    /// <summary>
    /// Gets or sets the current transaction context.
    /// </summary>
    TContext Context { get; set; }
}
