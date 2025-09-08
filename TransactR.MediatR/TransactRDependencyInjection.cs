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
        public ITransactionContextBuilder<TState, TTransactionContext> Create<TState, TTransactionContext>(TransactorBuilderOptions options)
            where TState : class, IState, new()
            where TTransactionContext : class, ITransactionContext<TTransactionContext, TState>, new()
        {
            return new MediatRTransactionContextBuilder<TState, TTransactionContext>(options);
        }
    }

    /// <summary>
    /// MediatR's concrete implementation of the transaction context builder.
    /// It contains the actual logic for the Surround method.
    /// </summary>
    internal class MediatRTransactionContextBuilder<TState, TTransactionContext>
        : TransactorBuilder<TState>, ITransactionContextBuilder<TState, TTransactionContext>
        where TState : class, IState, new()
        where TTransactionContext : class, ITransactionContext<TTransactionContext, TState>, new()
    {
        public MediatRTransactionContextBuilder(TransactorBuilderOptions options) : base(options) { }

        public ITransactionContextBuilder<TState, TTransactionContext> Surround<TRequest>()
            where TRequest : ITransactionalRequest<TState>
        {
            var requestType = typeof(TRequest);
            var responseType = GetResponseTypeFromRequest(requestType);

            if (responseType == null)
            {
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the MediatR 'IRequest<TResponse>' interface.");
            }

            var pipelineBehaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var transactionalBehaviorImplementationType = typeof(TransactionalBehavior<,,,>).MakeGenericType(requestType, responseType, typeof(TTransactionContext), typeof(TState));

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
