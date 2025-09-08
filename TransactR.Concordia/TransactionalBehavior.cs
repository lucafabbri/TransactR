using Concordia;
using Concordia.Behaviors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactR.Exceptions;

namespace TransactR.Concordia;

/// <summary>
/// An integration behavior for Concordia that adds TransactR's transactional capabilities
/// to the Concordia pipeline by hooking into the OnInbound and OnOutbound methods.
/// </summary>
public class TransactionalBehavior<TRequest, TResponse, TTransactionContext, TState>
    : ContextualPipelineBehavior<TRequest, TResponse, TransactRConcordiaContext<TTransactionContext>>
    where TRequest : ITransactionalRequest<TState>, IRequest<TResponse>
    where TTransactionContext : class, ITransactionContext<TTransactionContext, TState>, new()
    where TState : class, IState, new()
{
    private readonly IMementoStore<TState> _mementoStore;
    private readonly IStateRestorer<TState> _stateRestorer;
    private readonly ILogger<TransactionalBehavior<TRequest, TResponse, TTransactionContext, TState>> _logger;

    public TransactionalBehavior(
        IMementoStore<TState> mementoStore,
        IStateRestorer<TState> stateRestorer,
        ILogger<TransactionalBehavior<TRequest, TResponse, TTransactionContext, TState>> logger)
    {
        _mementoStore = mementoStore;
        _stateRestorer = stateRestorer;
        _logger = logger;
    }

    /// <summary>
    /// Executed before the handler. This is where we create/load the transaction context and save the memento.
    /// </summary>
    protected override async Task OnInbound(TransactRConcordiaContext<TTransactionContext> concordiaContext, TRequest request, CancellationToken cancellationToken)
    {
        var latestMemento = await _mementoStore.GetLatestAsync(request.TransactionId, cancellationToken);
        var transactionContext = (latestMemento != null)
            ? new TTransactionContext().Hydrate(request.TransactionId, latestMemento.State)
            : new TTransactionContext().Initialize(request.TransactionId);

        concordiaContext.TransactionContext = transactionContext;

        _logger.LogInformation("Saving memento for transaction {TransactionId}, step {Step}.", transactionContext.TransactionId, transactionContext.State.Step);
        await _mementoStore.SaveAsync(transactionContext.TransactionId, transactionContext.State, cancellationToken);
    }

    /// <summary>
    /// Executed after the handler. This is where we evaluate the response, handle rollbacks, and clean up mementos.
    /// </summary>
    protected override async Task OnOutbound(TransactRConcordiaContext<TTransactionContext> concordiaContext, TResponse response, CancellationToken cancellationToken)
    {
        var transactionContext = concordiaContext.TransactionContext;
        var transactionId = transactionContext.TransactionId;

        if (!concordiaContext.IsSuccess)
        {
            _logger.LogError("An exception occurred. Initiating disaster recovery for transaction {TransactionId}.", transactionId);
            await ApplyRollbackPolicyAsync(request: default, transactionId, transactionContext.State.Step, cancellationToken); // We don't have the request here, but we can still apply policy
            return;
        }

        var outcome = transactionContext.EvaluateResponse(response);
        switch (outcome)
        {
            case TransactionOutcome.Completed:
                _logger.LogInformation("Transaction {TransactionId} completed. Removing all mementos.", transactionId);
                await _mementoStore.RemoveTransactionAsync(transactionId, cancellationToken);
                break;

            case TransactionOutcome.InProgress:
                _logger.LogInformation("Transaction {TransactionId} is in progress at step {Step}. Memento is preserved.", transactionId, transactionContext.State.Step);
                break;

            case TransactionOutcome.Failed:
                _logger.LogWarning("Transaction {TransactionId} failed at step {Step} based on response evaluation. Initiating rollback.", transactionId, transactionContext.State.Step);
                await ApplyRollbackPolicyAsync(request: default, transactionId, transactionContext.State.Step, cancellationToken, isLogicalFailure: true);
                throw new TransactionEvaluationFailedException($"The transaction outcome was evaluated as '{nameof(TransactionOutcome.Failed)}'.");
        }
    }
    private async Task ApplyRollbackPolicyAsync(TRequest request, string transactionId, IComparable currentStep, CancellationToken cancellationToken, bool isLogicalFailure = false)
    {
        var policy = request.RollbackPolicy;
        if (isLogicalFailure)
        {
            // For logical failures without an explicit policy, we default to rolling back the current step.
            policy = RollbackPolicy.RollbackToCurrentStep;
        }

        _logger.LogInformation("Applying rollback policy '{Policy}' for transaction {TransactionId}.", policy, transactionId);

        switch (policy)
        {
            case RollbackPolicy.RollbackToCurrentStep:
                await RollbackToStepAsync(transactionId, currentStep, cancellationToken);
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
    }

    private async Task RollbackToStepAsync(string transactionId, IComparable step, CancellationToken cancellationToken)
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
