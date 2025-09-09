namespace ConcordiaAndTransactor.Sample.Domain
{
    public class Counter
    {
        public string Id { get; set; }
        public int Value { get; private set; }
        private Counter(string id, int value) { 
            Id = id; 
            Value = value;
        }
        public static Counter Create(int value = 0) => new(Ulid.NewUlid().ToString(), value);
        public static Counter Create(string id, int value = 0) => new(id, value);
        public void Increment() => Value++;
        public void Decrement() => Value--;
        public void Reset() => Value = 0;
    }
}
