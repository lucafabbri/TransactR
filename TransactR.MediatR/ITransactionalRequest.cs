using MediatR;

namespace TransactR.MediatR;

public interface ITransactionalRequest<TResponse, TStep, TContext> : IRequest<TResponse>, ITransactionalRequest<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{

}

public abstract class TransactionalRequest<TResponse, TStep, TContext>(string transactionId, TStep step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) :
    ITransactionalRequest<TResponse, TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public string TransactionId { get; } = transactionId;
    public TStep Step { get; } = step;
    public RollbackPolicy RollbackPolicy { get; } = rollbackPolicy;
}

public abstract class NumericTransactionalRequest<TReponse, TContext>(string transactionId, int step = 0, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) :
    TransactionalRequest<TReponse, int, TContext>(transactionId, step, rollbackPolicy)
    where TContext : class, ITransactionContext<int, TContext>, new()
{
}

public abstract class StringTransactionalRequest<TReponse, TContext>(string transactionId, string step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) :
    TransactionalRequest<TReponse, string, TContext>(transactionId, step, rollbackPolicy)
    where TContext : class, ITransactionContext<string, TContext>, new()
{
}

public abstract class EnumTransactionalRequest<TReponse, TContext, TEnum>(string transactionId, TEnum step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) :
    TransactionalRequest<TReponse, TEnum, TContext>(transactionId, step, rollbackPolicy)
    where TEnum : notnull, Enum
    where TContext : class, ITransactionContext<TEnum, TContext>, new()
{
}

