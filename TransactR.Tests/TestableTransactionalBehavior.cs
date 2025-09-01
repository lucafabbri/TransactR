using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;

namespace TransactR.Tests.TestDoubles
{
    /// <summary>
    /// A testable wrapper around TransactionalBehaviorBase that exposes the ExecuteAsync method publicly.
    /// This is generic to support different context types in tests.
    /// </summary>
    public class TestableTransactionalBehavior<TContext> : TransactionalBehaviorBase<TestRequest, TestResponse, TContext, TestState, TestStep>
        where TContext : class, ITransactionContext<TContext, TestState, TestStep>, new()
    {
        public TestableTransactionalBehavior(
            IMementoStore<TestState, TestStep> mementoStore,
            IStateRestorer<TestState> stateRestorer,
            ITransactionContextProvider<TContext> contextProvider,
            ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, TContext, TestState, TestStep>> logger)
            : base(mementoStore, stateRestorer, contextProvider, logger)
        {
        }

        public Task<TestResponse> Execute(TestRequest request, Func<TContext, Task<TestResponse>> next, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(request, next, cancellationToken);
        }
    }
}

