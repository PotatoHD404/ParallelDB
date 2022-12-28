namespace DependencyExample2;

public interface IDependencyManager
{
    void AddOperation(int id, Action operation, params int[] dependencies);
    event EventHandler<OperationCompletedEventArgs> OperationCompleted;
    void Execute();
}