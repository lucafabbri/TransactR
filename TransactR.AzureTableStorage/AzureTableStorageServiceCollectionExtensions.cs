using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TransactR.AzureTableStorage;

/// <summary>
/// Provides extension methods to easily register TransactR's Azure Table Storage services.
/// </summary>
public static class AzureTableStorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Azure Table Storage implementation of <see cref="IMementoStore{TState, TStep}"/>
    /// in the dependency injection container.
    /// </summary>
    /// <typeparam name="TState">The type of the state to store.</typeparam>
    /// <typeparam name="TStep">The type of the transaction step identifier.</typeparam>
    /// <param name="services">The services collection.</param>
    /// <param name="connectionString">The Azure Table Storage connection string.</param>
    /// <param name="tableName">The name of the table to use for mementos.</param>
    /// <returns>The services collection to continue the chain.</returns>
    public static IServiceCollection AddAzureTableStorageMementoStore<TState, TStep>(
        this IServiceCollection services,
        string connectionString,
        string tableName = "mementos")
        where TStep : notnull, IComparable
        where TState : class, new()
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("The connection string cannot be null or empty.", nameof(connectionString));
        }

        // Register the TableClient as a singleton, as it is thread-safe and efficient to reuse.
        services.TryAddSingleton(sp => new TableClient(connectionString, tableName));

        // Register the IMementoStore implementation.
        services.TryAddScoped<IMementoStore<TState, TStep>>(sp =>
        {
            var tableClient = sp.GetRequiredService<TableClient>();
            return new AzureTableStoreMemento<TState, TStep>(tableClient, tableName);
        });

        return services;
    }
}
