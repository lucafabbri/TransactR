using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;

namespace TransactR.Tests.TestDoubles;

/// <summary>
/// A testable wrapper around TransactionalBehaviorBase that exposes the ExecuteAsync method publicly.
/// This is generic to support different context types in tests.
/// </summary>
public class TestableTransactionalBehavior<TContext> : TransactionalBehaviorBase<TestRequest, TestResponse, TContext, TestState>
    where TContext : class, ITransactionContext<TContext, TestState>, new()
{
    public TestableTransactionalBehavior(
        IMementoStore<TestState> mementoStore,
        IStateRestorer<TestState> stateRestorer,
        ITransactionContextProvider<TContext> contextProvider,
        ILogger<TransactionalBehaviorBase<TestRequest, TestResponse, TContext, TestState>> logger)
        : base(mementoStore, stateRestorer, contextProvider, logger)
    {
    }

    public Task<TestResponse> Execute(TestRequest request, Func<TContext, Task<TestResponse>> next, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(request, next, cancellationToken);
    }
}

