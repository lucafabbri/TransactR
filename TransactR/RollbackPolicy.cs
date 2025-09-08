namespace TransactR;

/// <summary>
/// Defines the disaster recovery policy to apply when an unhandled exception occurs during a transaction.
/// </summary>
public enum RollbackPolicy
{
    /// <summary>
    /// Rolls back to the state saved at the beginning of the current step. This is the default behavior.
    /// </summary>
    RollbackToCurrentStep,

    /// <summary>
    /// Rolls back to the very first step of the transaction. Requires the store to identify the first step.
    /// </summary>
    RollbackToBeginning,

    /// <summary>
    /// Deletes all saved states for the entire transaction, effectively cancelling it.
    /// </summary>
    DeleteTransactionState,
}

