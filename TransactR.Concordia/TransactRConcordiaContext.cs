using Concordia;

namespace TransactR.Concordia
{
    /// <summary>
    /// A custom Concordia pipeline context that carries the TransactR transaction context.
    /// This acts as a bridge between the Concordia pipeline and the TransactR logic.
    /// </summary>
    public class TransactRConcordiaContext<TTransactionContext> : ICommandPipelineContext
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the TransactR context associated with the current pipeline execution.
        /// </summary>
        public TTransactionContext TransactionContext { get; set; }

        public DateTime StartTime { get; set; }

        public string ErrorCode { get; set; }
    }
}
