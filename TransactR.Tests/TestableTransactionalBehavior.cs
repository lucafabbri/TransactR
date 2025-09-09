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
public class TestableTransactionalBehavior<TContext> : TransactionalBehaviorBase<TestRequest<TContext>, TestResponse, TestStep, TContext>
    where TContext : class, ITransactionContext<TestStep, TContext>, new()
{
    public TestableTransactionalBehavior(
        IMementoStore<TestStep, TContext> mementoStore,
        IStateRestorer<TestStep, TContext> stateRestorer,
        ITransactionContextProvider<TestStep, TContext> contextProvider,
        ILogger<TransactionalBehaviorBase<TestRequest<TContext>, TestResponse, TestStep, TContext>> logger)
        : base(mementoStore, stateRestorer, contextProvider, logger)
    {
    }

    public async Task<TestResponse> Execute(TestRequest<TContext> request, Func<TContext, Task<TestResponse>> next, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(request, next, cancellationToken);
    }
}

