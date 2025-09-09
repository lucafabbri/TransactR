namespace TransactR.Tests.TestDoubles;

/// <summary>
/// A test context specifically for multi-step sagas.
/// </summary>
public class TestSagaContext : EnumTransactionContext<TestStep, TestSagaContext>
{
    public override TestStep InitialStep => TestStep.StepOne;

    public int Value { get; internal set; }

    public void AdvanceToStep(TestStep nextStep)
    {
        TrySetStep(nextStep);
    }

    public override TransactionOutcome EvaluateResponse(object? response = null)
    {
        // Fail the transaction if the response indicates a failure.
        if (response is null || (response is not null && response is TestResponse testResponse && !testResponse.Success))
        {
            return TransactionOutcome.Failed;
        }

        // The saga is only complete when it reaches the final step.
        if (Step.Equals(TestStep.StepTwo))
        {
            return TransactionOutcome.Completed;
        }

        // Otherwise, it's still in progress.
        return TransactionOutcome.InProgress;
    }
}

