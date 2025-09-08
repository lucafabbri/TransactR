using ConcordiaAndTransactor.Sample.Application;
using ErrorOr;
using TransactR;

namespace ConcordiaAndTransactor.Sample.Contexts;

public class CountContext : TransactionContext<CountContext, CounterState>
{
    private int IntialNumericStep = 0;
    public override IComparable InitialStep => IntialNumericStep;

    public override TransactionOutcome EvaluateResponse(object? response = null)
    {
        if (response is not ErrorOr<int> errorOrIntResponse)
        {
            State.Errors.Add(State.InnerStep, Error.Unexpected(description: "Invalid response type."));
            return TransactionOutcome.Failed;
        }
        return errorOrIntResponse.Match(
            value =>
            {
                State.Value = value;
                return State.InnerStep == 2 // Assuming 3 steps: Increment, Decrement, Reset
                    ? TransactionOutcome.Completed
                    : TransactionOutcome.InProgress;
            },
            errors =>
            {
                foreach (var error in errors)
                {
                    State.Errors.Add(State.InnerStep, error);
                }
                return TransactionOutcome.Failed;
            });
    }
}