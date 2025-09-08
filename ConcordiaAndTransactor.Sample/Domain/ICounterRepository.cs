using ErrorOr;

namespace ConcordiaAndTransactor.Sample.Domain
{
    public interface ICounterRepository
    {
        Task<IEnumerable<Counter>> GetAllCountersAsync(CancellationToken cancellationToken);
        Task<ErrorOr<Counter>> GetCounterAsync(string id, CancellationToken cancellationToken);
        Task<Counter> CreateCounter(CancellationToken cancellationToken);
        Task<ErrorOr<Counter>> DeleteCounter(string id, CancellationToken cancellationToken);
        Task<ErrorOr<Counter>> UpdateCounter(string id, Counter counter, CancellationToken cancellationToken);
    }
}
