using ConcordiaAndTransactor.Sample.Application;
using ConcordiaAndTransactor.Sample.Contexts;
using TransactR;

internal class CounterStateRestorer : IStateRestorer<int, CountContext>
{
    public Task RestoreAsync(CountContext state, CancellationToken cancellationToken = default)
    {
        //do nothing, state is in-memory only
        return Task.CompletedTask;
    }
}