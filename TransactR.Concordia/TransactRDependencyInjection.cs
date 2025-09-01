using Concordia;
using Microsoft.Extensions.DependencyInjection;

namespace TransactR.Concordia
{
    public static class TransactRDependencyInjection
    {
        /// <summary>
        /// Registers the TransactR TransactionalBehavior in the Concordia pipeline.
        /// Ensure that implementations for IMementoStore and IStateRestorer are also registered.
        /// </summary>
        /// <param name="services">The IServiceCollection.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddTransactRConcordia(this IServiceCollection services)
        {
            // Register the behavior as an open generic.
            // The DI container will resolve it for each TRequest that implements ITransactionalRequest.
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionalBehavior<,,,,>));
            return services;
        }
    }
}
