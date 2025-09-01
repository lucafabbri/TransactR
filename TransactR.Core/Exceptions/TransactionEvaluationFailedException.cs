namespace TransactR.Exceptions
{
    /// <summary>
    /// Exception thrown when a transaction's outcome is evaluated as 'Failed' by the transaction context.
    /// </summary>
    public class TransactionEvaluationFailedException : Exception
    {
        public TransactionEvaluationFailedException(string message) : base(message) { }
    }
}
