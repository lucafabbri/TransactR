using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace TransactR.MediatR
{
    public static class TransactRMediatRDependencyInjection
    {
        /// <summary>
        /// Registers the MediatR-specific transaction context builder factory.
        /// This method should be called after AddTransactR() to enable MediatR integration.
        /// </summary>
        public static ITransactorBuilder OnMediatR(this ITransactorBuilder builder)
        {
            builder.Options.TransactionContextBuilderFactory = new MediatRTransactionContextBuilderFactory();
            return builder;
        }
    }

    /// <summary>
    /// The factory responsible for creating MediatR-specific transaction context builders.
    /// </summary>
    internal class MediatRTransactionContextBuilderFactory : ITransactionContextBuilderFactory
    {
        public ITransactionContextBuilder<TStep, TContext> Create<TStep, TContext>(TransactorBuilderOptions options)
            where TStep : notnull, IComparable
            where TContext : class, ITransactionContext<TStep, TContext>, new()
        {
            return new MediatRTransactionContextBuilder<TStep, TContext>(options);
        }
    }

    /// <summary>
    /// MediatR's concrete implementation of the transaction context builder.
    /// It contains the actual logic for the Surround method.
    /// </summary>
    internal class MediatRTransactionContextBuilder<TStep, TContext>
        : TransactorBuilder<TStep, TContext>, ITransactionContextBuilder<TStep, TContext>
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        public MediatRTransactionContextBuilder(TransactorBuilderOptions options) : base(options) { }

        public ITransactionContextBuilder<TStep, TContext> Surround<TRequest>()
            where TRequest : ITransactionalRequest<TStep, TContext>
        {
            var requestType = typeof(TRequest);
            return Surround(requestType);
        }

        public ITransactionContextBuilder<TStep, TContext> Surround(Type requestType)
        {
            if (!typeof(ITransactionalRequest<TStep, TContext>).IsAssignableFrom(requestType))
            {
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the 'ITransactionalRequest<{typeof(TStep).FullName},{typeof(TContext).FullName}>' interface.");
            }
            var responseType = GetResponseTypeFromRequest(requestType);
            if (responseType == null)
            {
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the MediatR 'IRequest<TResponse>' interface.");
            }
            var pipelineBehaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var transactionalBehaviorImplementationType = typeof(TransactionalBehavior<,,,>).MakeGenericType(requestType, responseType, typeof(TStep), typeof(TContext));
            Options.Services.AddTransient(pipelineBehaviorInterfaceType, transactionalBehaviorImplementationType);
            return this;
        }

        private static Type? GetResponseTypeFromRequest(Type requestType)
        {
            // MediatR's IRequest<T> is in the MediatR namespace.
            var iRequestInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            return iRequestInterface?.GetGenericArguments().FirstOrDefault();
        }
    }
}
