using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TransactR.EntityFramework;

public class EntityFrameworkMementoStore<TDbContext, TStep, TContext> : IMementoStore<TStep, TContext>
    where TDbContext : DbContext
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    private readonly TDbContext _context;

    public EntityFrameworkMementoStore(TDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(string transactionId, TContext state, CancellationToken cancellationToken = default)
    {
        var serializedState = JsonSerializer.Serialize(state);

        var entity = await _context.Set<MementoEntity<TStep, TContext>>()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(state.Step.ToString()), cancellationToken);

        if (entity == null)
        {
            entity = new MementoEntity<TStep, TContext>
            {
                TransactionId = transactionId,
                Step = state.Step,
                State = serializedState
            };
            _context.Set<MementoEntity<TStep, TContext>>().Add(entity);
        }
        else
        {
            entity.State = serializedState;
            _context.Set<MementoEntity<TStep, TContext>>().Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TContext?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TStep, TContext>>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step.ToString()), cancellationToken);

        if (entity == null || entity.State == null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<TContext>(entity.State);
    }

    public async Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TStep, TContext>>()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step.ToString()), cancellationToken);

        if (entity != null)
        {
            _context.Set<MementoEntity<TStep, TContext>>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var states = await _context.Set<MementoEntity<TStep, TContext>>()
            .Where(e => e.TransactionId == transactionId)
            .ToListAsync(cancellationToken);

        return states.Select(e => JsonSerializer.Deserialize<TContext>(e.State).Step)
            .Min();
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        await _context.Set<MementoEntity<TStep, TContext>>()
            .Where(e => e.TransactionId == transactionId)
#if NET9_0_OR_GREATER
            .ExecuteDeleteAsync(cancellationToken);
#else
            .ForEachAsync(e => _context.Set<MementoEntity<TStep, TContext>>().Remove(e), cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
#endif
    }

    public async Task<Memento<TStep, TContext>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TStep, TContext>>()
            .Where(e => e.TransactionId == transactionId)
            .OrderByDescending(e => e.Step)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null || entity.State == null)
        {
            return null;
        }

        var state = JsonSerializer.Deserialize<TContext>(entity.State);

        if (state == null)
        {
            return null;
        }

        return new Memento<TStep, TContext>(state);
    }
}
