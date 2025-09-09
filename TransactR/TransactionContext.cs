using System;

namespace TransactR;

/// <summary>
/// Provides the base implementation for a transaction context.
/// Consumers must inherit from this class to provide a concrete evaluation logic.
/// </summary>
public abstract class TransactionContext<TStep, TContext> : ITransactionContext<TStep, TContext>
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public string TransactionId { get; protected set; } = string.Empty;
    public abstract TStep InitialStep { get; protected set; }
    public TStep Step { get; protected set; } = default!;

    /// <summary>
    /// When implemented in a derived class, evaluates the response from the handler
    /// to determine the outcome of the transaction step.
    /// </summary>
    public abstract TransactionOutcome EvaluateResponse(object? response = null);

    public void Initialize(string transactionId)
    {
        TransactionId = transactionId;
    }


    public abstract bool TryIncrementStep();
    public abstract bool TryDecrementStep();
    public bool TrySetStep(TStep step)
    {
        if (step is null) 
            return false;
        Step = step;
        return true;
    }
}

public abstract class NumericTransactionContext<TContext> : TransactionContext<int, TContext>
    where TContext : class, ITransactionContext<int, TContext>, new()
{
    public override int InitialStep { get; protected set; } = 0;
    public NumericTransactionContext() : this(0)
    {
    }
    public NumericTransactionContext(int initialStep)
    {
        InitialStep = initialStep;
        Step = initialStep;
    }
    public override bool TryIncrementStep()
    {
        Step++;
        return true;
    }
    public override bool TryDecrementStep()
    {
        if (Step == 0)
        {
            return false;
        }
        Step--;
        return true;
    }
}

public abstract class StringTransactionContext<TContext> : TransactionContext<string, TContext>
    where TContext : class, ITransactionContext<string, TContext>, new()
{
    public abstract string[] Steps { get; }
    public override string InitialStep { get; protected set; } = string.Empty;
    public StringTransactionContext()
    {
        InitialStep = Steps.Length > 0 ? Steps[0] : string.Empty;
        Step = InitialStep;
    }
    public StringTransactionContext(string initialStep)
    {
        InitialStep = initialStep;
        Step = initialStep;
    }
    public override bool TryIncrementStep()
    {
        var currentIndex = Array.IndexOf(Steps, Step);
        if (currentIndex < 0 || currentIndex + 1 >= Steps.Length)
        {
            return false;
        }
        Step = Steps[currentIndex + 1];
        return true;
    }
    public override bool TryDecrementStep()
    {
        var currentIndex = Array.IndexOf(Steps, Step);
        if (currentIndex <= 0)
        {
            return false;
        }
        Step = Steps[currentIndex - 1];
        return true;
    }
}

public abstract class EnumTransactionContext<TEnum, TContext> : TransactionContext<TEnum, TContext>
    where TEnum : struct, Enum
    where TContext : class, ITransactionContext<TEnum, TContext>, new()
{
    public override TEnum InitialStep { get; protected set; } = default!;

    public EnumTransactionContext()
    {
        var values = Enum.GetValues(typeof(TEnum));
        InitialStep = values.Length > 0 ? (TEnum)values.GetValue(0)! : default!;
        Step = InitialStep;
    }

    public EnumTransactionContext(TEnum initialStep)
    {
        InitialStep = initialStep;
        Step = initialStep;
    }
    public override bool TryIncrementStep()
    {
        var values = Enum.GetValues(typeof(TEnum));
        var currentIndex = Array.IndexOf(values, Step);

        if (currentIndex < 0)
        {
            Step = default!;
            return true;
        }

        if (currentIndex + 1 >= values.Length)
        {
            return false;
        }

        Step = (TEnum)values.GetValue(currentIndex + 1);

        return true;
    }

    public override bool TryDecrementStep()
    {
        var values = Enum.GetValues(typeof(TEnum));
        var currentIndex = Array.IndexOf(values, Step);

        if (currentIndex <= 0)
        {
            return false;
        }

        Step = (TEnum)values.GetValue(currentIndex - 1);

        return true;
    }
}

