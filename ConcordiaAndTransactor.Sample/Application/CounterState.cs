using ErrorOr;
using TransactR;

namespace ConcordiaAndTransactor.Sample.Application;

public class CounterState : NumericState
{
    public int Value { get; set; }
    public Dictionary<int, Error> Errors { get; set; } = [];
}