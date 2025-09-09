namespace TransactR;

public class EmptyStateRestorer<TStep, TContext> : IStateRestorer<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public Task RestoreAsync(TContext state, CancellationToken cancellationToken = default)
    {
        // No operation performed
        return Task.CompletedTask;
    }
}
