namespace DependencyExample2;

public class DependencyManager : IDependencyManager
{
    private readonly Dictionary<int, OperationData> _operations = new();

    private readonly object _stateLock = new();
    private Dictionary<int, List<int>> _dependenciesFromTo = new();
    private volatile int _remainingCount;
    private ManualResetEvent done = new(false);

    public event EventHandler<OperationCompletedEventArgs> OperationCompleted = delegate { };

    public void AddOperation(
        int id, Action operation, params int[] dependencies)
    {
        if (operation == null)
            throw new ArgumentNullException("operation");
        if (dependencies == null)
            throw new ArgumentNullException("dependencies");

        var data = new OperationData
        {
            Context = ExecutionContext.Capture(),
            Id = id,
            Operation = operation,
            Dependencies = dependencies
        };
        _operations.Add(id, data);
    }

    public void Execute()
    {
        InternalExecute();
        lock (_stateLock)
        {
            foreach (OperationData op in _operations.Values)
            {
                if (op.NumRemainingDependencies == 0)
                    QueueOperation(op);
            }
        }
    }

    public void ExecuteAndWait()
    {
        InternalExecute();
        // Launch and wait
        _remainingCount = _operations.Count;
        using (done = new ManualResetEvent(false))
        {
            lock (_stateLock)
            {
                foreach (OperationData op in _operations.Values)
                {
                    if (op.NumRemainingDependencies == 0)
                        QueueOperation(op);
                }
            }

            done.WaitOne();
        }
    }

    private void InternalExecute()
    {
        VerifyThatAllOperationsHaveBeenRegistered();
        VerifyThereAreNoCycles();
        // Fill dependency data structures
        _dependenciesFromTo = new Dictionary<int, List<int>>();
        foreach (OperationData op in _operations.Values)
        {
            op.NumRemainingDependencies = op.Dependencies.Length;

            foreach (int from in op.Dependencies)
            {
                List<int> toList;
                if (!_dependenciesFromTo.TryGetValue(from, out toList))
                {
                    toList = new List<int>();
                    _dependenciesFromTo.Add(from, toList);
                }

                toList.Add(op.Id);
            }
        }
    }


    private void QueueOperation(OperationData data)
    {
        ThreadPool.UnsafeQueueUserWorkItem(state =>
            ProcessOperation((OperationData)state), data);
    }

    private void ProcessOperation(OperationData data)
    {
        // Time and run the operation's delegate
        data.Start = DateTimeOffset.Now;
        if (data.Context != null)
        {
            ExecutionContext.Run(data.Context.CreateCopy(),
                op => ((OperationData)op).Operation(), data);
        }
        else data.Operation();

        data.End = DateTimeOffset.Now;


        // Raise the operation completed event
        OnOperationCompleted(data);

        // Signal to all that depend on this operation of its
        // completion, and potentially launch newly available
        lock (_stateLock)
        {
            List<int> toList;
            if (_dependenciesFromTo.TryGetValue(data.Id, out toList))
            {
                foreach (int targetId in toList)
                {
                    OperationData targetData = _operations[targetId];
                    if (--targetData.NumRemainingDependencies == 0)
                        QueueOperation(targetData);
                }
            }

            _dependenciesFromTo.Remove(data.Id);


            if (--_remainingCount == 0) done.Set();
        }
    }

    private void OnOperationCompleted(OperationData data)
    {
        EventHandler<OperationCompletedEventArgs> handler = OperationCompleted;
        if (handler != null)
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
        if (CreateTopologicalSort() == null)
            throw new InvalidOperationException("Cycle detected");
    }

    private List<int> CreateTopologicalSort()
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
                List<int> ids;
                if (!dependenciesFromTo.TryGetValue(depId, out ids))
                {
                    ids = new List<int>();
                    dependenciesFromTo.Add(depId, ids);
                }

                ids.Add(op.Id);
            }
        }

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
                    List<int> depIds;
                    if (dependenciesFromTo.TryGetValue(item.Key, out depIds))
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