namespace TransactR;

public interface IState
{
    IComparable Step { get; }
    bool TryIncrementStep();
    bool TryDecrementStep();
    bool TrySetStep(IComparable step);
}

public abstract class State<T> : IState
    where T : notnull, IComparable
{
    public IComparable Step => InnerStep;

    public T InnerStep { get; protected set; } = default!;

    public State() : this(default!)
    {
    }

    public State(T step)
    {
        InnerStep = step;
    }

    public abstract bool TryIncrementStep();
    public abstract bool TryDecrementStep();
    public bool TrySetStep(IComparable step) => SetStepInternal(step);

    protected virtual bool SetStepInternal(object step) => step is T tStep && (InnerStep = tStep) != null;
}

public abstract class NumericState : State<int>
{
    public NumericState() : this(0)
    {
    }

    public NumericState(int step) : base(step)
    {
    }

    public override bool TryIncrementStep()
    {
        InnerStep++;
        return true;
    }

    public override bool TryDecrementStep()
    {
        if (InnerStep == 0)
        {
            return false;
        }
        InnerStep--;
        return true;
    }
}

public abstract class StringState : State<string>
{
    public abstract string[] Steps { get; }

    public StringState() : this(string.Empty)
    {
    }

    public StringState(string step) : base(step)
    {
        step ??= Steps.FirstOrDefault();
        if (Steps.Length == 0)
        {
            InnerStep = null;
        }
        else if (Array.IndexOf(Steps, step) < 0)
        {
            InnerStep = Steps.First();
        }
    }

    public override bool TryIncrementStep()
    {
        var currentIndex = Array.IndexOf(Steps, InnerStep);

        if (currentIndex < 0)
        {
            InnerStep = Steps.FirstOrDefault() ?? default!;
            return true;
        }

        if (currentIndex + 1 >= Steps.Length)
        {
            return false;
        }
        
        InnerStep = Steps[currentIndex + 1];
        
        return true;
    }

    public override bool TryDecrementStep()
    {
        var currentIndex = Array.IndexOf(Steps, InnerStep);
        
        if (currentIndex <= 0)
        {
            return false;
        }

        InnerStep = Steps[currentIndex - 1];
        
        return true;
    }

    protected override bool SetStepInternal(object step)
    {
        return step is string sStep && Array.IndexOf(Steps, sStep) >= 0 && base.SetStepInternal(sStep);
    }
}

public abstract class EnumState<TEnum> : State<TEnum>
    where TEnum : notnull, Enum
{
    public EnumState() : this(default!)
    {
    }

    public EnumState(TEnum step) : base(step)
    {
    }

    public override bool TryIncrementStep()
    {
        var values = Enum.GetValues(typeof(TEnum));
        var currentIndex = Array.IndexOf(values, InnerStep);

        if (currentIndex < 0)
        {
            InnerStep = default!;
            return true;
        }

        if(currentIndex + 1 >= values.Length)
        {
            return false;
        }

        InnerStep = (TEnum)values.GetValue(currentIndex + 1);

        return true;
    }

    public override bool TryDecrementStep()
    {
        var values = Enum.GetValues(typeof(TEnum));
        var currentIndex = Array.IndexOf(values, InnerStep);

        if (currentIndex <= 0)
        {
            return false;
        }

        InnerStep = (TEnum)values.GetValue(currentIndex - 1);

        return true;
    }

    protected override bool SetStepInternal(object step)
    {
        return step is TEnum eStep && Array.IndexOf(Enum.GetValues(typeof(TEnum)), eStep) >= 0 && base.SetStepInternal(step);
    }
}
