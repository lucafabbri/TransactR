using ConcordiaAndTransactor.Sample.Application;
using ErrorOr;
using TransactR;

namespace ConcordiaAndTransactor.Sample.Contexts;

public class CountContext : NumericTransactionContext<CountContext>
{
    public int Value { get; set; }
    public Dictionary<int, Error> Errors { get; set; } = [];
    private int IntialNumericStep = 0;
    public override int InitialStep => IntialNumericStep;

    public override TransactionOutcome EvaluateResponse(object? response = null)
    {
        if (response is not ErrorOr<int> errorOrIntResponse)
        {
            Errors.Add(Step, Error.Unexpected(description: "Invalid response type."));
            return TransactionOutcome.Failed;
        }
        return errorOrIntResponse.Match(
            value =>
            {
                Value = value;
                return Step == 2 // Assuming 3 steps: Increment, Decrement, Reset
                    ? TransactionOutcome.Completed
                    : TransactionOutcome.InProgress;
            },
            errors =>
            {
                foreach (var error in errors)
                {
                    Errors.Add(Step, error);
                }
                return TransactionOutcome.Failed;
            });
    }
}