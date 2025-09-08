using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TransactR.EntityFramework;

public class EntityFrameworkMementoStore<TDbContext, TState> : IMementoStore<TState>
    where TDbContext : DbContext
    where TState : class, IState, new()
{
    private readonly TDbContext _context;

    public EntityFrameworkMementoStore(TDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(string transactionId, TState state, CancellationToken cancellationToken = default)
    {
        var serializedState = JsonSerializer.Serialize(state);

        var entity = await _context.Set<MementoEntity<TState>>()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(state.Step.ToString()), cancellationToken);

        if (entity == null)
        {
            entity = new MementoEntity<TState>
            {
                TransactionId = transactionId,
                Step = state.Step.ToString(),
                State = serializedState
            };
            _context.Set<MementoEntity<TState>>().Add(entity);
        }
        else
        {
            entity.State = serializedState;
            _context.Set<MementoEntity<TState>>().Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TState?> RetrieveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TState>>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step.ToString()), cancellationToken);

        if (entity == null || entity.State == null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<TState>(entity.State);
    }

    public async Task RemoveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TState>>()
            .FirstOrDefaultAsync(e => e.TransactionId == transactionId && e.Step!.Equals(step.ToString()), cancellationToken);

        if (entity != null)
        {
            _context.Set<MementoEntity<TState>>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IComparable?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var states = await _context.Set<MementoEntity<TState>>()
            .Where(e => e.TransactionId == transactionId)
            .ToListAsync(cancellationToken);

        return states.Select(e => JsonSerializer.Deserialize<TState>(e.State)?.Step)
            .Min();
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        await _context.Set<MementoEntity<TState>>()
            .Where(e => e.TransactionId == transactionId)
#if NET9_0_OR_GREATER
            .ExecuteDeleteAsync(cancellationToken);
#else
            .ForEachAsync(e => _context.Set<MementoEntity<TState>>().Remove(e), cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
#endif
    }

    public async Task<Memento<TState>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<MementoEntity<TState>>()
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

        return new Memento<TState>(state);
    }
}
