
namespace TransactR.Tests.TestDoubles;

// A simple, concrete state object for testing.
public class TestState : EnumState<TestStep>
{
    public int Value { get; set; }

    public TestState() : base(TestStep.StepOne) { }
    public TestState(TestStep currentStep) : base(currentStep) { }
}

// A simple enum for transaction steps.
public enum TestStep
{
    StepOne = 1,
    StepTwo = 2
}

// A concrete request that only depends on TransactR interfaces.
public class TestRequest : ITransactionalRequest<TestState>
{
    public string TransactionId { get; set; }

    public IComparable Step => TestStep.StepOne;

    public RollbackPolicy RollbackPolicy => RollbackPolicy.RollbackToCurrentStep;
}

// A simple response object.
public class TestResponse
{
    public bool Success { get; set; }
}

