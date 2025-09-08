using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TransactR;

public class TransactorBuilderOptions
{
    public IServiceCollection Services { get; private set; }

    public ITransactionContextBuilderFactory? TransactionContextBuilderFactory { get; set; }

    public TransactorBuilderOptions(IServiceCollection services)
    {
        Services = services;
    }
}


public interface ITransactionContextBuilderFactory
{
    ITransactionContextBuilder<TState, TTransactionContext> Create<TState, TTransactionContext>(TransactorBuilderOptions options)
        where TState : class, IState, new()
        where TTransactionContext : class, ITransactionContext<TTransactionContext, TState>, new();
}