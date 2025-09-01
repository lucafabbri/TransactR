using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Exceptions;

namespace TransactR.Behaviors
{
    /// <summary>
    /// Provides the core logic for transactional behavior, including memento saving and rollback on exception.
    /// This class is library-agnostic and designed to be extended by specific pipeline implementations (e.g., for MediatR).
    /// </summary>
    public abstract class TransactionalBehaviorBase<TRequest, TResponse, TContext, TState, TStep>
        where TRequest : ITransactionalRequest<TState, TStep>
        where TContext : class, ITransactionContext<TContext, TState, TStep>, new()
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        private readonly IMementoStore<TState, TStep> _mementoStore;
        private readonly IStateRestorer<TState> _stateRestorer;
        private readonly ITransactionContextProvider<TContext> _contextProvider;
        private readonly ILogger<TransactionalBehaviorBase<TRequest, TResponse, TContext, TState, TStep>> _logger;

        protected TransactionalBehaviorBase(
            IMementoStore<TState, TStep> mementoStore,
            IStateRestorer<TState> stateRestorer,
            ITransactionContextProvider<TContext> contextProvider,
            ILogger<TransactionalBehaviorBase<TRequest, TResponse, TContext, TState, TStep>> logger)
        {
            _mementoStore = mementoStore ?? throw new ArgumentNullException(nameof(mementoStore));
            _stateRestorer = stateRestorer ?? throw new ArgumentNullException(nameof(stateRestorer));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the transactional logic, providing a transaction context to the next delegate in the pipeline.
        /// </summary>
        protected async Task<TResponse> ExecuteAsync(
            TRequest request,
            Func<TContext, Task<TResponse>> next,
            CancellationToken cancellationToken)
        {
            var latestMemento = await _mementoStore.GetLatestAsync(request.TransactionId, cancellationToken);
            ITransactionContext<TContext, TState, TStep> transactionContext;

            if (latestMemento != null)
            {
                _logger.LogInformation("Continuing transaction {TransactionId}. Loading state from step {Step}.", request.TransactionId, latestMemento.Step);
                transactionContext = new TContext().Hydrate(request.TransactionId, latestMemento.Step, latestMemento.State);
            }
            else
            {
                _logger.LogInformation("Starting new transaction {TransactionId}.", request.TransactionId);
                transactionContext = new TContext().Initialize(request.TransactionId);
            }

            _contextProvider.Context = (TContext)transactionContext;

            var transactionId = transactionContext.TransactionId;
            var stepToProtect = transactionContext.Step;
            var stateToProtect = transactionContext.State;

            _logger.LogInformation("Saving memento for transaction {TransactionId}, step {Step}.", transactionId, stepToProtect);
            await _mementoStore.SaveAsync(transactionId, stepToProtect, stateToProtect, cancellationToken);

            try
            {
                var response = await next((TContext)transactionContext);
                var outcome = transactionContext.EvaluateResponse(response);

                switch (outcome)
                {
                    case TransactionOutcome.Completed:
                        _logger.LogInformation("Saga {TransactionId} completed. Removing all related mementos.", transactionId);
                        await _mementoStore.RemoveTransactionAsync(transactionId, cancellationToken);
                        break;
                    case TransactionOutcome.InProgress:
                        _logger.LogInformation("Transaction {TransactionId} is in progress at step {Step}. Memento is preserved.", transactionId, transactionContext.Step);
                        break;
                    case TransactionOutcome.Failed:
                        _logger.LogWarning("Transaction {TransactionId} failed at step {Step} based on response evaluation. Initiating rollback.", transactionId, transactionContext.Step);
                        throw new TransactionEvaluationFailedException($"The transaction outcome was evaluated as '{nameof(TransactionOutcome.Failed)}'.");
                    default:
                        throw new InvalidOperationException($"Unknown TransactionOutcome: {outcome}");
                }
                return response;
            }
            catch (Exception ex)
            {
                if (ex is not TransactionEvaluationFailedException)
                {
                    _logger.LogError(ex, "An error occurred during transaction {TransactionId}, step {Step}. Initiating disaster recovery.", transactionId, stepToProtect);
                }

                var policy = (request is ITransactionalRequestWithPolicy<TState, TStep> p) ? p.RollbackPolicy : RollbackPolicy.RollbackToCurrentStep;
                _logger.LogInformation("Applying rollback policy '{Policy}' for transaction {TransactionId}.", policy, transactionId);

                switch (policy)
                {
                    case RollbackPolicy.RollbackToCurrentStep:
                        await RollbackToStepAsync(transactionId, stepToProtect, cancellationToken);
                        break;
                    case RollbackPolicy.RollbackToBeginning:
                        var firstStep = await _mementoStore.GetFirstStepAsync(transactionId, cancellationToken);
                        if (firstStep != null && !firstStep.Equals(default))
                        {
                            await RollbackToStepAsync(transactionId, firstStep, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("Could not determine the first step for transaction {TransactionId}. Cannot roll back to beginning.", transactionId);
                        }
                        break;
                    case RollbackPolicy.DeleteTransactionState:
                        await _mementoStore.RemoveTransactionAsync(transactionId, cancellationToken);
                        _logger.LogInformation("All mementos for transaction {TransactionId} have been deleted.", transactionId);
                        break;
                }
                throw;
            }
        }

        private async Task RollbackToStepAsync(string transactionId, TStep step, CancellationToken cancellationToken)
        {
            var previousState = await _mementoStore.RetrieveAsync(transactionId, step, cancellationToken);
            if (previousState != null)
            {
                _logger.LogInformation("Restoring state for transaction {TransactionId} from step {Step}.", transactionId, step);
                await _stateRestorer.RestoreAsync(previousState, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Could not find a memento to restore for transaction {TransactionId}, step {Step}.", transactionId, step);
            }
        }
    }
}

