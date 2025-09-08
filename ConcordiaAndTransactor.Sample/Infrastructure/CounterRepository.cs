using ConcordiaAndTransactor.Sample.Domain;
using ErrorOr;
using System.Collections.Concurrent;

namespace ConcordiaAndTransactor.Sample.Infrastructure
{
    public class CounterRepository : ICounterRepository
    {
        private readonly ConcurrentDictionary<string, Counter> _counters = new();

        public Task<Counter> CreateCounter(CancellationToken cancellationToken)
        {
            var counter = Counter.Create();
            var id = Ulid.NewUlid().ToString();
            _counters[id] = counter;
            return Task.FromResult(counter);
        }

        public Task<ErrorOr<Counter>> DeleteCounter(string id, CancellationToken cancellationToken)
        {
            if (_counters.TryRemove(id, out var counter))
            {
                return Task.FromResult<ErrorOr<Counter>>(counter);
            }
            return Task.FromResult<ErrorOr<Counter>>(Error.NotFound("Counter.NotFound", $"Counter with id {id} not found."));
        }

        public Task<IEnumerable<Counter>> GetAllCountersAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Counter>>(_counters.Values);
        }

        public Task<ErrorOr<Counter>> GetCounterAsync(string id, CancellationToken cancellationToken)
        {
            if (_counters.TryGetValue(id, out var counter))
            {
                return Task.FromResult<ErrorOr<Counter>>(counter);
            }
            return Task.FromResult<ErrorOr<Counter>>(Error.NotFound("Counter.NotFound", $"Counter with id {id} not found."));
        }

        public Task<ErrorOr<Counter>> UpdateCounter(string id, Counter counter, CancellationToken cancellationToken)
        {
            _counters[id] = counter;
            return Task.FromResult<ErrorOr<Counter>>(counter);
        }
    }
}
