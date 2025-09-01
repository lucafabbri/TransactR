using TransactR.Tests.TestDoubles;

namespace TransactR.Tests
{
    /// <summary>
    /// A test context for simple, single-step transactions that are
    /// expected to complete immediately.
    /// </summary>
    public class SingleStepTestContext : TransactionContext<SingleStepTestContext, TestState, TestStep>
    {
        public override TestStep InitialStep => TestStep.StepOne;

        public override TransactionOutcome EvaluateResponse<TResponse>(TResponse response)
        {
            // For this context, the transaction is always considered complete after the first step.
            return TransactionOutcome.Completed;
        }
    }
}
