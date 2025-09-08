using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace TransactR.DistributedMemoryCache;

public class DistributedMemoryCacheMementoStore<TState> : IMementoStore<TState>
    where TState : class, IState, new()
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _options;

    public DistributedMemoryCacheMementoStore(
        IDistributedCache cache,
        DistributedCacheEntryOptions? options = null)
    {
        _cache = cache;
        _options = options ?? new DistributedCacheEntryOptions();
    }

    private string GetKey(string transactionId, IComparable step) => $"memento:{transactionId}:{step}";
    private string GetLatestKey(string transactionId) => $"memento-latest:{transactionId}";
    private string GetFirstKey(string transactionId) => $"memento-first:{transactionId}";

    public async Task SaveAsync(string transactionId, TState state, CancellationToken cancellationToken = default)
    {
        var serializedState = JsonSerializer.Serialize(state);
        var key = GetKey(transactionId, state.Step);
        await _cache.SetStringAsync(key, serializedState, _options, cancellationToken);
        await _cache.SetStringAsync(GetLatestKey(transactionId), JsonSerializer.Serialize(new Memento<TState>(state)), _options, cancellationToken);
    }

    public async Task<TState?> RetrieveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        var key = GetKey(transactionId, step);
        var serializedState = await _cache.GetStringAsync(key, cancellationToken);
        if (serializedState is null) return null;
        return JsonSerializer.Deserialize<TState>(serializedState);
    }

    public Task RemoveAsync(string transactionId, IComparable step, CancellationToken cancellationToken = default)
    {
        var key = GetKey(transactionId, step);
        return _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<IComparable?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var serializedMemento = await _cache.GetStringAsync(GetFirstKey(transactionId), cancellationToken);
        if (serializedMemento is null) return default;
        var memento = JsonSerializer.Deserialize<Memento<TState>>(serializedMemento);
        return memento.State.Step;
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // L'eliminazione di un'intera transazione richiederebbe una convenzione sulle chiavi
        // o un'operazione di pulizia. Questa è una semplificazione.
        await _cache.RemoveAsync(GetLatestKey(transactionId), cancellationToken);
        await _cache.RemoveAsync(GetFirstKey(transactionId), cancellationToken);
        // Per le chiavi individuali si userebbe un pattern di naming e un'operazione di bulk delete non standard.
    }

    public async Task<Memento<TState>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var serializedMemento = await _cache.GetStringAsync(GetLatestKey(transactionId), cancellationToken);
        if (serializedMemento is null) return null;
        return JsonSerializer.Deserialize<Memento<TState>>(serializedMemento);
    }
}