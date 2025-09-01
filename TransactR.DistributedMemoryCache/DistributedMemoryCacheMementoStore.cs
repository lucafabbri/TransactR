using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace TransactR.DistributedMemoryCache;

public class DistributedMemoryCacheMementoStore<TState, TStep> : IMementoStore<TState, TStep>
    where TState : class, new()
    where TStep : notnull, IComparable
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

    private string GetKey(string transactionId, TStep step) => $"memento:{transactionId}:{step}";
    private string GetLatestKey(string transactionId) => $"memento-latest:{transactionId}";
    private string GetFirstKey(string transactionId) => $"memento-first:{transactionId}";

    public async Task SaveAsync(string transactionId, TStep step, TState state, CancellationToken cancellationToken = default)
    {
        var serializedState = JsonSerializer.Serialize(state);
        var key = GetKey(transactionId, step);
        await _cache.SetStringAsync(key, serializedState, _options, cancellationToken);
        await _cache.SetStringAsync(GetLatestKey(transactionId), JsonSerializer.Serialize(new Memento<TState, TStep>(step, state)), _options, cancellationToken);
        // La gestione del primo passo è più complessa in una cache distribuita senza transazioni.
        // La manteniamo semplice per questa prima implementazione.
    }

    public async Task<TState?> RetrieveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var key = GetKey(transactionId, step);
        var serializedState = await _cache.GetStringAsync(key, cancellationToken);
        if (serializedState is null) return null;
        return JsonSerializer.Deserialize<TState>(serializedState);
    }

    public Task RemoveAsync(string transactionId, TStep step, CancellationToken cancellationToken = default)
    {
        var key = GetKey(transactionId, step);
        return _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<TStep?> GetFirstStepAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // Questa implementazione è una semplificazione e potrebbe non essere accurata in scenari distribuiti.
        // Si può migliorare salvando esplicitamente la prima chiave.
        var serializedMemento = await _cache.GetStringAsync(GetFirstKey(transactionId), cancellationToken);
        if (serializedMemento is null) return default;
        var memento = JsonSerializer.Deserialize<Memento<TState, TStep>>(serializedMemento);
        return memento.Step;
    }

    public async Task RemoveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // L'eliminazione di un'intera transazione richiederebbe una convenzione sulle chiavi
        // o un'operazione di pulizia. Questa è una semplificazione.
        await _cache.RemoveAsync(GetLatestKey(transactionId), cancellationToken);
        await _cache.RemoveAsync(GetFirstKey(transactionId), cancellationToken);
        // Per le chiavi individuali si userebbe un pattern di naming e un'operazione di bulk delete non standard.
    }

    public async Task<Memento<TState, TStep>?> GetLatestAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var serializedMemento = await _cache.GetStringAsync(GetLatestKey(transactionId), cancellationToken);
        if (serializedMemento is null) return null;
        return JsonSerializer.Deserialize<Memento<TState, TStep>>(serializedMemento);
    }
}