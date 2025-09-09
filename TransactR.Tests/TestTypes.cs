
namespace TransactR.Tests.TestDoubles;

// A simple enum for transaction steps.
public enum TestStep
{
    StepOne = 1,
    StepTwo = 2
}

// A concrete request that only depends on TransactR interfaces.
public class TestRequest<TContext> : ITransactionalRequest<TestStep, TContext>
    where TContext : class, ITransactionContext<TestStep, TContext>, new()
{
    public string TransactionId { get; set; }

    public TestStep Step => TestStep.StepOne;

    public RollbackPolicy RollbackPolicy => RollbackPolicy.RollbackToCurrentStep;
}

// A simple response object.
public class TestResponse
{
    public bool Success { get; set; }
}

