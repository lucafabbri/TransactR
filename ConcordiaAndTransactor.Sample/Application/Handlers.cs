using Concordia;
using ConcordiaAndTransactor.Sample.Domain;
using ErrorOr;
using TransactR;

namespace ConcordiaAndTransactor.Sample.Application;

public class CreateCounterCommandHandler : IRequestHandler<CreateCounterCommand, ErrorOr<Counter>>
{
    private readonly ICounterRepository _counterRepository;

    public CreateCounterCommandHandler(ICounterRepository counterRepository)
    {
        _counterRepository = counterRepository;
    }

    public async Task<ErrorOr<Counter>> Handle(CreateCounterCommand request, CancellationToken cancellationToken)
    {
        return await _counterRepository.CreateCounter(cancellationToken);
    }
}

public class GetCounterQueryHandler : IRequestHandler<GetCounterQuery, ErrorOr<Counter>>
{
    private readonly ICounterRepository _counterRepository;
    public GetCounterQueryHandler(ICounterRepository counterRepository)
    {
        _counterRepository = counterRepository;
    }
    public async Task<ErrorOr<Counter>> Handle(GetCounterQuery request, CancellationToken cancellationToken)
    {
        return await _counterRepository.GetCounterAsync(request.Id, cancellationToken);
    }
}

public class GetAllCountersQueryHandler : IRequestHandler<GetAllCountersQuery, IEnumerable<Counter>>
{
    private readonly ICounterRepository _counterRepository;
    public GetAllCountersQueryHandler(ICounterRepository counterRepository)
    {
        _counterRepository = counterRepository;
    }
    public async Task<IEnumerable<Counter>> Handle(GetAllCountersQuery request, CancellationToken cancellationToken)
    {
        return await _counterRepository.GetAllCountersAsync(cancellationToken);
    }
}   

public class IncrementCommandHandler : IRequestHandler<IncrementCommand, ErrorOr<int>>
{
    private readonly ICounterRepository _counterService;

    public IncrementCommandHandler(ICounterRepository counterService)
    {
        _counterService = counterService;
    }

    public async Task<ErrorOr<int>> Handle(IncrementCommand request, CancellationToken cancellationToken)
    {
        return await _counterService.GetCounterAsync(request.Id, cancellationToken)
            .ThenAsync(async counter =>
            {
                counter.Increment();
                return await _counterService.UpdateCounter(request.Id, counter, cancellationToken)
                    .Then(_ => _.Value);
            });

    }
}

public class DecrementCommandHandler : IRequestHandler<DecrementCommand, ErrorOr<int>>
{
    private readonly ICounterRepository _counterService;
    public DecrementCommandHandler(ICounterRepository counterService)
    {
        _counterService = counterService;
    }
    public async Task<ErrorOr<int>> Handle(DecrementCommand request, CancellationToken cancellationToken)
    {
        return await _counterService.GetCounterAsync(request.Id, cancellationToken)
            .ThenAsync(async counter =>
            {
                counter.Decrement();
                return await _counterService.UpdateCounter(request.Id, counter, cancellationToken)
                    .Then(_ => _.Value);
            });
    }
}

public class ResetCommandHandler : IRequestHandler<ResetCommand, ErrorOr<int>>
{
    private readonly ICounterRepository _counterService;
    public ResetCommandHandler(ICounterRepository counterService)
    {
        _counterService = counterService;
    }
    public async Task<ErrorOr<int>> Handle(ResetCommand request, CancellationToken cancellationToken)
    {
        return await _counterService.GetCounterAsync(request.Id, cancellationToken)
            .ThenAsync(async counter =>
            {
                counter.Reset();
                return await _counterService.UpdateCounter(request.Id, counter, cancellationToken)
                    .Then(_ => _.Value);
            });
    }
}
