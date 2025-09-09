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
        public ITransactionContextBuilder<TStep, TContext> Create<TStep, TContext>(TransactorBuilderOptions options)
            where TStep : notnull, IComparable
            where TContext : class, ITransactionContext<TStep, TContext>, new()
        {
            return new ConcordiaTransactionContextBuilder<TStep, TContext>(options);
        }
    }

    internal class ConcordiaTransactionContextBuilder<TStep, TContext>
        : TransactorBuilder<TStep, TContext>, ITransactionContextBuilder<TStep, TContext>
        where TStep : notnull, IComparable
        where TContext : class, ITransactionContext<TStep, TContext>, new()
    {
        public ConcordiaTransactionContextBuilder(TransactorBuilderOptions options) : base(options) { }

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
                throw new ArgumentException($"Type '{requestType.FullName}' must implement the 'IRequest<TResponse>' interface.");
            }
            
            var pipelineBehaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var transactionalBehaviorImplementationType = typeof(TransactionalBehavior<,,,>).MakeGenericType(requestType, responseType, typeof(TStep), typeof(TContext));
            
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

