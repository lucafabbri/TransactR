using TransactR.Tests.TestDoubles;

namespace TransactR.Tests;

/// <summary>
/// A test context for simple, single-step transactions that are
/// expected to complete immediately.
/// </summary>
public class SingleStepTestContext : EnumTransactionContext<TestStep, SingleStepTestContext>
{
    public override TestStep InitialStep => TestStep.StepOne;

    public int Value { get; internal set; }

    public override TransactionOutcome EvaluateResponse(object? response = null)
    {
        // For this context, the transaction is always considered complete after the first step.
        return TransactionOutcome.Completed;
    }
}
