using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TransactR.EntityFramework
{
    public class EntityFrameworkMementoStore<TDbContext, TState, TStep> : IMementoStore<TState, TStep>
        where TDbContext : DbContext
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        private readonly TDbContext _context;

        public EntityFrameworkMementoStore(TDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(string transactionId, TStep step, TState state, CancellationToken cancellationToken = default)
        {
            var serializedState = JsonSerializer.Serialize(state);

            var entity = await _context.Set<MementoEntity<TState, TStep>>()
                .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step), cancellationToken);

            if (entity == null)
            {
                entity = new MementoEntity<TState, TStep>
                {
                    TransactionId = transactionId,
                    Step = step,
                    State = serializedState
                };
                _context.Set<MementoEntity<TState, TStep>>().Add(entity);
            }
            else
            {
                entity.State = serializedState;
                _context.Set<MementoEntity<TState, TStep>>().Update(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<TState?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<MementoEntity<TState, TStep>>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step), cancellationToken);

            if (entity == null || entity.State == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<TState>(entity.State);
        }

        public async Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<MementoEntity<TState, TStep>>()
                .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step), cancellationToken);

            if (entity != null)
            {
                _context.Set<MementoEntity<TState, TStep>>().Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<MementoEntity<TState, TStep>>()
                .Where(e => e.TransactionId == transactionId)
                .Select(e => e.Step)
                .MinAsync(cancellationToken);
        }

        public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            await _context.Set<MementoEntity<TState, TStep>>()
                .Where(e => e.TransactionId == transactionId)
#if NET9_0_OR_GREATER
                .ExecuteDeleteAsync(cancellationToken);
#else
                .ForEachAsync(e => _context.Set<MementoEntity<TState, TStep>>().Remove(e), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
#endif
        }

        public async Task<Memento<TState, TStep>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<MementoEntity<TState, TStep>>()
                .Where(e => e.TransactionId == transactionId)
                .OrderByDescending(e => e.Step)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null || entity.State == null)
            {
                return null;
            }

            var state = JsonSerializer.Deserialize<TState>(entity.State);

            if (state == null)
            {
                return null;
            }

            return new Memento<TState, TStep>(entity.Step!, state);
        }
    }
}
