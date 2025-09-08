using Concordia;

namespace TransactR.Concordia;

public interface ITransactionalRequest<TResponse, TState> : IRequest<TResponse>, ITransactionalRequest<TState>
    where TState : class, IState, new()
{
}

public abstract class TransactionalRequest<TResponse, TState>(string transactionId, IComparable step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : 
    ITransactionalRequest<TResponse, TState>
    where TState : class, IState, new()
{
    public string TransactionId { get; } = transactionId;
    public IComparable Step { get; } = step;
    public RollbackPolicy RollbackPolicy { get; } = rollbackPolicy;
}

public abstract class  NumericTransactionalRequest<TReponse, TState>(string transactionId, int step = 0, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : 
    TransactionalRequest<TReponse, TState>(transactionId, step, rollbackPolicy)
    where TState : class, IState, new()
{
}

public abstract class StringTransactionalRequest<TReponse, TState>(string transactionId, string step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : 
    TransactionalRequest<TReponse, TState>(transactionId, step, rollbackPolicy)
    where TState : class, IState, new()
{
}

public abstract class EnumTransactionalRequest<TReponse, TState, TEnum>(string transactionId, TEnum step, RollbackPolicy rollbackPolicy = RollbackPolicy.RollbackToCurrentStep) : 
    TransactionalRequest<TReponse, TState>(transactionId, step, rollbackPolicy)
    where TEnum : notnull, Enum
    where TState : class, IState, new()
{
}