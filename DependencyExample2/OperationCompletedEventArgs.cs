namespace DependencyExample2;

public class OperationCompletedEventArgs : EventArgs
{
    internal OperationCompletedEventArgs(
        int id, DateTimeOffset start, DateTimeOffset end)
    {
        Id = id;
        Start = start;
        End = end;
    }

    public int Id { get; private set; }
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }
}