using Concordia;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace TransactR.Concordia
{
    public static class TransactRConcordiaDependencyInjection
    {
        public static ITransactorBuilder OnConcordia(this ITransactorBuilder builder)
        {
            builder.Options.TransactionContextBuilderFactory = new ConcordiaTransactionContextBuilderFactory();
            return builder;
        }
    }

    internal class ConcordiaTransactionContextBuilderFactory : ITransactionContextBuilderFactory
    {
        public ITransactionContextBuilder<TState, TTransactionContext> Create<TState, TTransactionContext>(TransactorBuilderOptions options)
            where TState : class, IState, new()
            where TTransactionContext : class, ITransactionContext<TState>, new()
        {
            return new ConcordiaTransactionContextBuilder<TState, TTransactionContext>(options);
        }
    }

    internal class ConcordiaTransactionContextBuilder<TState, TTransactionContext>
        : TransactorBuilder<TState>, ITransactionContextBuilder<TState, TTransactionContext>
        where TState : class, IState, new()
        where TTransactionContext : class, ITransactionContext<TState>, new()
    {
        public ConcordiaTransactionContextBuilder(TransactorBuilderOptions options) : base(options) { }

        public ITransactionContextBuilder<TState, TTransactionContext> Surround<TRequest>()
            where TRequest : ITransactionalRequest<TState>
        {
            var requestType = typeof(TRequest);
            return Surround(requestType);
        }

        public ITransactionContextBuilder<TState, TTransactionContext> Surround(Type requestType)
        {
            if (!typeof(ITransactionalRequest<TState>).IsAssignableFrom(requestType))
            {
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the 'ITransactionalRequest<{typeof(TState).FullName}>' interface.");
            }
            
            var responseType = GetResponseTypeFromRequest(requestType);
            
            if (responseType == null)
            {
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the 'IRequest<TResponse>' interface.");
            }
            
            var pipelineBehaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var transactionalBehaviorImplementationType = typeof(TransactionalBehavior<,,,>).MakeGenericType(requestType, responseType, typeof(TTransactionContext), typeof(TState));
            
            Options.Services.AddTransient(pipelineBehaviorInterfaceType, transactionalBehaviorImplementationType);
            
            return this;
        }

        private static Type? GetResponseTypeFromRequest(Type requestType)
        {
            var iRequestInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            return iRequestInterface?.GetGenericArguments().FirstOrDefault();
        }
    }
}

