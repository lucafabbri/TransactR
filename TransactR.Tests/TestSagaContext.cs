namespace TransactR.Tests.TestDoubles
{
    /// <summary>
    /// A test context specifically for multi-step sagas.
    /// </summary>
    public class TestSagaContext : TransactionContext<TestSagaContext, TestState, TestStep>
    {
        public override TestStep InitialStep => TestStep.StepOne;

        public void AdvanceToStep(TestStep nextStep)
        {
            Step = nextStep;
        }

        public override TransactionOutcome EvaluateResponse<TResponse>(TResponse response)
        {
            // Fail the transaction if the response indicates a failure.
            if (response is TestResponse testResponse && !testResponse.Success)
            {
                return TransactionOutcome.Failed;
            }

            // The saga is only complete when it reaches the final step.
            if (Step == TestStep.StepTwo)
            {
                return TransactionOutcome.Completed;
            }

            // Otherwise, it's still in progress.
            return TransactionOutcome.InProgress;
        }
    }
}

