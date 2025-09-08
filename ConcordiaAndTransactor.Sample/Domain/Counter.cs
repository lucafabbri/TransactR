namespace ConcordiaAndTransactor.Sample.Domain
{
    public class Counter
    {
        public int Value { get; private set; }
        private Counter(int value) { Value = value; }
        public static Counter Create() => new(0);
        public void Increment() => Value++;
        public void Decrement() => Value--;
        public void Reset() => Value = 0;
    }
}
