namespace DependencyExample2;

internal class OperationData
{
    internal ExecutionContext? Context;
    internal int[] Dependencies;
    internal DateTimeOffset End;
    internal int Id;
    internal int NumRemainingDependencies;
    internal Func<object> Operation;
    internal DateTimeOffset Start;
}