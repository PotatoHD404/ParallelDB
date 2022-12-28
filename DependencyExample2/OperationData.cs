using System.Collections.Concurrent;

namespace DependencyExample2;

internal class OperationData
{
    internal int[] Dependencies;
    internal DateTimeOffset End;
    internal int Id;
    internal int NumRemainingDependencies;
    internal Func<ConcurrentDictionary<int, dynamic?>, dynamic?> Operation;
    internal DateTimeOffset Start;
    
    internal OperationData(int id, Func<ConcurrentDictionary<int, dynamic?>, dynamic?> operation, int[] dependencies)
    {
        Id = id;
        Operation = operation;
        Dependencies = dependencies;
        NumRemainingDependencies = dependencies.Length;
    }
}