using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Behaviors;

namespace TransactR.MediatR
{
    public class TransactionalBehavior<TRequest, TResponse, TContext, TState, TStep>
        : TransactionalBehaviorBase<TRequest, TResponse, TContext, TState, TStep>, IPipelineBehavior<TRequest, TResponse>
        where TRequest : ITransactionalRequest<TState, TStep>, IRequest<TResponse>
        where TContext : class, ITransactionContext<TContext, TState, TStep>, new()
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        public TransactionalBehavior(
            IMementoStore<TState, TStep> mementoStore,
            IStateRestorer<TState> stateRestorer,
            ITransactionContextProvider<TContext> contextProvider,
            ILogger<TransactionalBehaviorBase<TRequest, TResponse, TContext, TState, TStep>> logger)
            : base(mementoStore, stateRestorer, contextProvider, logger)
        {
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // The MediatR pipeline does not pass the context down, so we wrap the 'next' delegate.
            // The context will be created/loaded and passed to the provider by ExecuteAsync.
            Func<TContext, Task<TResponse>> nextWrapper = (context) => next();

            return ExecuteAsync(request, nextWrapper, cancellationToken);
        }
    }
}

