using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TransactR.EntityFramework;

/// <summary>
/// Fornisce metodi di estensione per registrare i servizi di Entity Framework di TransactR.
/// </summary>
public static class TransactRServiceCollectionExtensions
{
    /// <summary>
    /// Registra l'implementazione dello store di memento in Entity Framework nel contenitore di servizi.
    /// </summary>
    /// <typeparam name="TDbContext">Il tipo di DbContext.</typeparam>
    /// <typeparam name="TState">Il tipo di stato del memento.</typeparam>
    /// <typeparam name="TStep">Il tipo di passo del memento.</typeparam>
    /// <param name="services">La collezione di servizi.</param>
    /// <returns>La collezione di servizi.</returns>
    public static ITransactorBuilder<TStep, TContext> PeristedEfCore<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDbContext,
        TStep, TContext>(this ITransactorBuilder<TStep, TContext> transactorBuilder)
        where TDbContext : DbContext
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()

    {
        transactorBuilder.Options.Services.AddScoped<IMementoStore<TStep, TContext>, EntityFrameworkMementoStore<TDbContext, TStep, TContext>>();
        return transactorBuilder;
    }

    /// <summary>
    /// Registra le entità di TransactR nel contesto di Entity Framework Core usando il tipo di stato e passo specificato.
    /// Questo metodo usa il meccanismo di sostituzione del servizio IModelCustomizer.
    /// </summary>
    /// <typeparam name="TDbContext">Il tipo di DbContext.</typeparam>
    /// <typeparam name="TState">Il tipo di stato del memento.</typeparam>
    /// <typeparam name="TStep">Il tipo di passo del memento.</typeparam>
    /// <param name="builder">Il builder usato per configurare il contesto di Entity Framework.</param>
    /// <returns>Il builder del contesto di Entity Framework.</returns>
    public static DbContextOptionsBuilder UseTransactR<TDbContext, TStep, TContext>(this DbContextOptionsBuilder builder)
        where TDbContext : DbContext
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return ReplaceService<IModelCustomizer, TransactRModelCustomizer<TDbContext, TStep, TContext>>(builder);

        static DbContextOptionsBuilder ReplaceService<TService, TImplementation>(DbContextOptionsBuilder builder)
            where TImplementation : TService
            => builder.ReplaceService<TService, TImplementation>();
    }

    /// <summary>
    /// Registra le entità di TransactR nel contesto di Entity Framework Core usando il tipo di stato e passo specificato.
    /// Questo metodo si integra direttamente con ModelBuilder.
    /// </summary>
    /// <typeparam name="TState">Il tipo di stato del memento.</typeparam>
    /// <typeparam name="TStep">Il tipo di passo del memento.</typeparam>
    /// <param name="builder">Il builder usato per configurare il modello di Entity Framework.</param>
    /// <returns>Il builder del modello di Entity Framework.</returns>
    public static ModelBuilder UseTransactR<TStep, TContext>(this ModelBuilder builder)
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var customizer = new TransactRModelCustomizer<DbContext, TStep, TContext>();
        customizer.Customize(builder, null);
        return builder;
    }
}
