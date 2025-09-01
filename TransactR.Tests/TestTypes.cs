namespace TransactR.Tests.TestDoubles
{
    // A simple, concrete state object for testing.
    public class TestState
    {
        public int Value { get; set; }
    }

    // A simple enum for transaction steps.
    public enum TestStep
    {
        StepOne = 1,
        StepTwo = 2
    }

    // A concrete request that only depends on TransactR interfaces.
    public class TestRequest : ITransactionalRequest<TestState, TestStep>
    {
        public string TransactionId { get; set; }
    }

    // A concrete request that also specifies a rollback policy.
    public class TestRequestWithPolicy : TestRequest, ITransactionalRequestWithPolicy<TestState, TestStep>
    {
        public RollbackPolicy RollbackPolicy { get; set; }
    }

    // A simple response object.
    public class TestResponse
    {
        public bool Success { get; set; }
    }
}

