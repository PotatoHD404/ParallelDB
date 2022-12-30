using System.Collections.Concurrent;

namespace ParallelDB.Dependencies;

internal class OperationData
{
    internal readonly int[] Dependencies;
    internal DateTimeOffset End;
    internal readonly int Id;
    internal int NumRemainingDependencies;
    internal int NumRemainingSuccessors;
    internal ExecutionContext? Context;
    internal readonly Func<ConcurrentDictionary<int, dynamic?>, dynamic?> Operation;
    internal DateTimeOffset Start;
    
    internal OperationData(int id, Func<ConcurrentDictionary<int, dynamic?>, dynamic?> operation, int[] dependencies, ExecutionContext? context = null)
    {
        Id = id;
        Operation = operation;
        Dependencies = dependencies;
        NumRemainingDependencies = dependencies.Length;
        NumRemainingSuccessors = 0;
        Context = context;
    }
}