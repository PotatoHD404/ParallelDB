using System.Collections.Concurrent;

namespace DependencyExample2;

public interface IDependencyManager
{
    void AddOperation<T>(int id, Func<ConcurrentDictionary<int, dynamic?>, T> operation, params int[] dependencies);
    event EventHandler<OperationCompletedEventArgs> OperationCompleted;
    void Execute();
    void ExecuteAndWait();
    ConcurrentDictionary<int, dynamic?> GetResults();
}