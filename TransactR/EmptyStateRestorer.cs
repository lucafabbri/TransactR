namespace TransactR;

/// <summary>
/// A no-operation implementation of IStateRestorer.
/// This can be used when no state restoration is needed.
/// </summary>
/// <typeparam name="TState">The type of state to restore.</typeparam>
/// <seealso cref="IStateRestorer{TState}"/>
public class EmptyStateRestorer<TState> : IStateRestorer<TState>
    where TState : class, IState, new()
{
    public Task RestoreAsync(TState state, CancellationToken cancellationToken = default)
    {
        // No operation performed
        return Task.CompletedTask;
    }
}
