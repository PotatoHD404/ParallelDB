using System.Collections.Concurrent;

namespace ParallelDB.Dependencies;

public class DependencyManager : IDependencyManager
{
    private readonly ConcurrentDictionary<int, OperationData> _operations = new();

    private readonly ConcurrentDictionary<int, List<int>> _dependenciesFromTo = new();
    private readonly ConcurrentDictionary<int, List<int>> _dependenciesToFrom = new();
    private volatile int _remainingCount;
    private ManualResetEvent _done = new(false);
    private readonly ConcurrentDictionary<int, dynamic?> _results = new();

    public event EventHandler<OperationCompletedEventArgs>? OperationCompleted;

    public void AddOperation<T>(
        int id, Func<ConcurrentDictionary<int, dynamic?>, T> operation, params int[] dependencies)
    {
        if (operation is null)
            throw new ArgumentNullException(nameof(operation));
        if (dependencies is null)
            throw new ArgumentNullException(nameof(dependencies));

        foreach (var dep in dependencies)
        {
            if (!_operations.ContainsKey(dep))
                throw new ArgumentException($"Dependency {dep} does not exist", nameof(dependencies));
            Interlocked.Increment(ref _operations[dep].NumRemainingSuccessors);
        }

        var data = new OperationData(id, res => operation(res), dependencies);
        _operations.TryAdd(id, data);
        Interlocked.Increment(ref _remainingCount);
    }

    public void Execute()
    {
        InternalExecute();

        foreach (OperationData op in _operations.Values)
        {
            if (op.NumRemainingDependencies == 0)
                QueueOperation(op);
        }
    }

    public ConcurrentDictionary<int, dynamic?> GetResults()
    {
        InternalExecute();
        // Launch and wait
        using (_done = new ManualResetEvent(false))
        {
            foreach (OperationData op in _operations.Values)
            {
                if (op.NumRemainingDependencies == 0)
                    QueueOperation(op);
            }

            _done.WaitOne();
        }

        return _results;
    }

    public void ExecuteAndWait()
    {
        GetResults();
    }

    private void InternalExecute()
    {
        VerifyThatAllOperationsHaveBeenRegistered();
        VerifyThereAreNoCycles();

        foreach (OperationData op in _operations.Values)
        {
            op.NumRemainingDependencies = op.Dependencies.Length;

            foreach (int from in op.Dependencies)
            {
                if (!_dependenciesFromTo.TryGetValue(from, out var toList))
                {
                    toList = new List<int>();
                    _dependenciesFromTo.TryAdd(from, toList);
                }
                toList.Add(op.Id);
                if (!_dependenciesToFrom.TryGetValue(op.Id, out var fromList))
                {
                    fromList = new List<int>();
                    _dependenciesToFrom.TryAdd(op.Id, fromList);
                }
                fromList.Add(from);
                _results.TryAdd(op.Id, null);
            }
        }
    }


    private void QueueOperation(OperationData data)
    {
        ThreadPool.UnsafeQueueUserWorkItem(state =>
            ProcessOperation((OperationData)state!), data);
    }

    private void ProcessOperation(OperationData data)
    {
        // Time and run the operation's delegate
        data.Start = DateTimeOffset.Now;
        var result = data.Operation(_results);

        data.End = DateTimeOffset.Now;

        _results[data.Id] = result;
        // Raise the operation completed event
        OnOperationCompleted(data);

        // Signal to all that depend on this operation of its
        // completion, and potentially launch newly available

        if (_dependenciesFromTo.TryGetValue(data.Id, out var toList))
        {
            foreach (int targetId in toList)
            {
                OperationData targetData = _operations[targetId];
                if (Interlocked.Decrement(ref targetData.NumRemainingDependencies) == 0)
                    QueueOperation(targetData);
            }
        }
        
        _dependenciesFromTo.TryRemove(data.Id, out _);
        
        if(_dependenciesToFrom.TryGetValue(data.Id, out var fromList))
        {
            foreach (int fromId in fromList)
            {
                OperationData fromData = _operations[fromId];
                if (Interlocked.Decrement(ref fromData.NumRemainingSuccessors) == 0)
                    _results.TryRemove(fromId, out _);
            }
        }
        
        _operations.TryRemove(data.Id, out _);

        if (Interlocked.Decrement(ref _remainingCount) == 0) _done.Set();
    }

    private void OnOperationCompleted(OperationData data)
    {
        EventHandler<OperationCompletedEventArgs>? handler = OperationCompleted;
        if (handler is not null)
            handler(this, new OperationCompletedEventArgs(
                data.Id, data.Start, data.End));
    }


    private void VerifyThatAllOperationsHaveBeenRegistered()
    {
        foreach (OperationData op in _operations.Values)
        {
            foreach (int dependency in op.Dependencies)
            {
                if (!_operations.ContainsKey(dependency))
                {
                    throw new InvalidOperationException(
                        "Missing operation: " + dependency);
                }
            }
        }
    }

    private void VerifyThereAreNoCycles()
    {
        if (CreateTopologicalSort() is null)
            throw new InvalidOperationException("Cycle detected");
    }

    private List<int>? CreateTopologicalSort()
    {
        // Build up the dependencies graph
        var dependenciesToFrom = new Dictionary<int, List<int>>();
        var dependenciesFromTo = new Dictionary<int, List<int>>();

        foreach (OperationData op in _operations.Values)
        {
            // Note that op.Id depends on each of op.Dependencies
            dependenciesToFrom.Add(op.Id, new List<int>(op.Dependencies));

            // Note that each of op.Dependencies is relied on by op.Id
            foreach (int depId in op.Dependencies)
            {
                if (!dependenciesFromTo.TryGetValue(depId, out var ids))
                {
                    ids = new List<int>();
                    dependenciesFromTo.Add(depId, ids);
                }

                ids.Add(op.Id);
            }
        }

        // print dependencies
        // foreach (var item in dependenciesToFrom)
        // {
        //     Console.WriteLine($"Key: {item.Key}, Value: {string.Join(",", item.Value)}");
        // }
        // Console.WriteLine();
        // foreach (var item in dependenciesFromTo)
        // {
        //     Console.WriteLine($"Key: {item.Key}, Value: {string.Join(",", item.Value)}");
        // }
        // Console.WriteLine();
        // Create the sorted list
        var overallPartialOrderingIds = new List<int>(dependenciesToFrom.Count);
        var thisIterationIds = new List<int>(dependenciesToFrom.Count);

        while (dependenciesToFrom.Count > 0)
        {
            thisIterationIds.Clear();
            foreach (var item in dependenciesToFrom)
            {
                // If an item has zero input operations, remove it.
                if (item.Value.Count == 0)
                {
                    thisIterationIds.Add(item.Key);

                    // Remove all outbound edges
                    if (dependenciesFromTo.TryGetValue(item.Key, out var depIds))
                    {
                        foreach (int depId in depIds)
                        {
                            dependenciesToFrom[depId].Remove(item.Key);
                        }
                    }
                }
            }

            // If nothing was found to remove, there's no valid sort.
            if (thisIterationIds.Count == 0) return null;

            // Remove the found items from the dictionary and 
            // add them to the overall ordering
            foreach (int id in thisIterationIds) dependenciesToFrom.Remove(id);
            overallPartialOrderingIds.AddRange(thisIterationIds);
        }

        return overallPartialOrderingIds;
    }
}