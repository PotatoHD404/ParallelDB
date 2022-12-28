namespace DependencyExample;

public class OperationData
{
    internal readonly int Id;
    internal readonly Action Operation;
    internal readonly int[] Dependencies;
    internal readonly ExecutionContext Context;
    internal int NumRemainingDependencies;
    internal DateTimeOffset Start, End;
    
    public OperationData(int id, Action operation, int[] dependencies, ExecutionContext? context)
    {
        Id = id;
        Operation = operation;
        Dependencies = dependencies;
        Context = context ?? throw new ArgumentNullException(nameof(context));
        NumRemainingDependencies = dependencies.Length;
    }
}