using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TransactR.MediatR
{
    public static class TransactRDependencyInjection
    {
        /// <summary>
        /// Registra il TransactionalBehavior di TransactR nella pipeline di MediatR.
        /// Assicurati di registrare anche le implementazioni di IMementoStore, IStateExtractor e IStateRestorer.
        /// </summary>
        /// <param name="services">La IServiceCollection.</param>
        /// <returns>La IServiceCollection per il chaining.</returns>
        public static IServiceCollection AddTransactRMediatR(this IServiceCollection services)
        {
            // Registra il behavior come un open generic. 
            // Il container DI lo risolverà per ogni TRequest che implementa ITransactionalRequest.
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionalBehavior<,,,,>));
            return services;
        }

        /// <summary>
        /// Registra l'implementazione in-memory di IMementoStore come singleton.
        /// Utile per test o applicazioni semplici.
        /// </summary>
        /// <typeparam name="TState">Il tipo di stato.</typeparam>
        /// <typeparam name="TStep">Il tipo di step.</typeparam>
        /// <param name="services">La IServiceCollection.</param>
        /// <returns>La IServiceCollection per il chaining.</returns>
        public static IServiceCollection AddTransactRInMemoryStore<TState, TStep>(this IServiceCollection services)
            where TStep : notnull, IComparable
            where TState : class, new()
        {
            services.AddSingleton<IMementoStore<TState, TStep>, InMemoryMementoStore<TState, TStep>>();
            return services;
        }
    }
}