namespace TransactR
{
    /// <summary>
    /// A default, concrete implementation of ITransactionContextProvider.
    /// </summary>
    /// <typeparam name="TContext">The type of the transaction context.</typeparam>
    public class TransactionContextProvider<TContext> : ITransactionContextProvider<TContext>
    {
        /// <summary>
        /// Gets or sets the current transaction context.
        /// </summary>
        public TContext Context { get; set; }
    }
}

