using Concordia;
using ConcordiaAndTransactor.Sample.Contexts;
using ConcordiaAndTransactor.Sample.Domain;
using ErrorOr;
using TransactR.Concordia;

namespace ConcordiaAndTransactor.Sample.Application;

public class CreateCounterCommand : IRequest<ErrorOr<Counter>> { }

public record GetCounterQuery(string Id) : IRequest<ErrorOr<Counter>> { }

public record GetAllCountersQuery() : IRequest<IEnumerable<Counter>> { }

public class IncrementCommand : TransactionalRequest<ErrorOr<int>, int, CountContext>
{ 
    public string Id { get; set; }

    public IncrementCommand(string id, string transactionId) : base(transactionId, 0, TransactR.RollbackPolicy.RollbackToCurrentStep)
    {
        Id = id;
    }
}
public class DecrementCommand : TransactionalRequest<ErrorOr<int>, int, CountContext>
{
    public string Id { get; set; }

    public DecrementCommand(string id, string transactionId) : base(transactionId, 1, TransactR.RollbackPolicy.RollbackToCurrentStep)
    {
        Id = id;
    }
}
public class ResetCommand : TransactionalRequest<ErrorOr<int>, int, CountContext>
{
    public string Id { get; set; }

    public ResetCommand(string id, string transactionId) : base(transactionId, 2, TransactR.RollbackPolicy.RollbackToCurrentStep)
    {
        Id = id;
    }
}
