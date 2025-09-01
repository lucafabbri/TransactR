namespace TransactR
{
    /// <summary>
    /// Defines the possible outcomes of a transactional step after its response is evaluated.
    /// </summary>
    public enum TransactionOutcome
    {
        /// <summary>
        /// The step completed successfully. The memento for this step can be removed.
        /// </summary>
        Completed,

        /// <summary>
        /// The transaction is still in progress. The memento for this step should be preserved for potential future rollbacks.
        /// </summary>
        InProgress,

        /// <summary>
        /// The step has failed based on business logic evaluation. A rollback should be initiated.
        /// </summary>
        Failed
    }
}

