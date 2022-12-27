namespace DependencyExample;

public class OperationData
{
    internal int Id;
    internal Action Operation;
    internal int[] Dependencies;
    internal ExecutionContext Context;
    internal int NumRemainingDependencies;
    internal DateTimeOffset Start, End;
}