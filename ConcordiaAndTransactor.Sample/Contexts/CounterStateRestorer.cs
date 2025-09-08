using ConcordiaAndTransactor.Sample.Application;
using TransactR;

internal class CounterStateRestorer : IStateRestorer<CounterState>
{
    public Task RestoreAsync(CounterState state, CancellationToken cancellationToken = default)
    {
        //do nothing, state is in-memory only
        return Task.CompletedTask;
    }
}