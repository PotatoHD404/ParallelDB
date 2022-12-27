namespace DependencyExample;

public class DependencyManager
{
    private Dictionary<int, OperationData> _operations = new();
    private Dictionary<int, List<int>> _dependenciesFromTo = new();
    private object _stateLock = new();
    private ManualResetEvent _doneEvent = new(false);
    private int _remainingCount = 0;

    public void AddOperation(int id, Action operation, params int[] dependencies)
    {
        if (operation == null) throw new ArgumentNullException("operation");
        if (dependencies == null) throw new ArgumentNullException("dependencies");
        var data = new OperationData
            { Context = ExecutionContext.Capture(), Id = id, Operation = operation, Dependencies = dependencies };
        _operations.Add(id, data);
    }

    public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

    public void Execute()
    {
        // TODO: verification will go here later
        // Fill dependency data structures
        _dependenciesFromTo = new Dictionary<int, List<int>>();
        foreach (var op in _operations.Values)
        {
            op.NumRemainingDependencies = op.Dependencies.Length;
            foreach (var from in op.Dependencies)
            {
                List<int> toList;
                if (!_dependenciesFromTo.TryGetValue(from, out toList))
                {
                    toList = new List<int>();
                    _dependenciesFromTo.Add(from, toList);
                }

                toList.Add(op.Id);
            }
        } // Launch and wait

        _remainingCount = _operations.Count;
        using (_doneEvent = new ManualResetEvent(false))
        {
            lock (_stateLock)
            {
                foreach (var op in _operations.Values)
                {
                    if (op.NumRemainingDependencies == 0) QueueOperation(op);
                }
            }

            _doneEvent.WaitOne();
        }
    }

    private void QueueOperation(OperationData data)
    {
        ThreadPool.UnsafeQueueUserWorkItem(state => ProcessOperation((OperationData)state), data);
    }

    private void ProcessOperation(OperationData data)
    {
        // Time and run the operation's delegate
        data.Start = DateTimeOffset.Now;
        ExecutionContext.Run(data.Context.CreateCopy(), op => ((OperationData)op).Operation(), data);

        data.End = DateTimeOffset
            .Now; // Raise the operation completed event
        OnOperationCompleted(data); // Signal to all that depend on this operation of its
        // completion, and potentially launch newly available
        lock (_stateLock)
        {
            List<int> toList;
            if (_dependenciesFromTo.TryGetValue(data.Id, out toList))
            {
                foreach (var targetId in toList)
                {
                    OperationData targetData = _operations[targetId];
                    if (--targetData.NumRemainingDependencies == 0) QueueOperation(targetData);
                }
            }

            _dependenciesFromTo.Remove(data.Id);
            if (--_remainingCount == 0) _doneEvent.Set();
        }
    }

    private void OnOperationCompleted(OperationData data)
    {
        var handler = OperationCompleted;
        handler(this, new OperationCompletedEventArgs(data.Id, data.Start, data.End));
    }

    public class OperationCompletedEventArgs : EventArgs
    {
        internal OperationCompletedEventArgs(int id, DateTimeOffset start, DateTimeOffset end)
        {
            Id = id;
            Start = start;
            End = end;
        }

        public int Id { get; private set; }
        public DateTimeOffset Start { get; private set; }
        public DateTimeOffset End { get; private set; }
    }

    private void VerifyThatAllOperationsHaveBeenRegistered()
    {
        foreach (var op in _operations.Values)
        {
            foreach (var dependency in op.Dependencies)
            {
                if (!_operations.ContainsKey(dependency))
                {
                    throw new InvalidOperationException("Missing operation: " + dependency);
                }
            }
        }
    }

    private void VerifyThereAreNoCycles()
    {
        if (CreateTopologicalSort() == null) throw new InvalidOperationException("Cycle detected");
    }

    private List<int>? CreateTopologicalSort()
    {
        // Build up the dependencies graph
        var dependenciesToFrom = new Dictionary<int, List<int>>();
        var dependenciesFromTo = new Dictionary<int, List<int>>();
        foreach (var op in _operations.Values)
        {
            // Note that op.Id depends on each of op.Dependencies
            dependenciesToFrom.Add(op.Id,
                new List<int>(op
                    .Dependencies)); // Note that each of op.Dependencies is relied on by op.Id
            foreach (var depId in op.Dependencies)
            {
                List<int> ids;
                if (!dependenciesFromTo.TryGetValue(depId, out ids))
                {
                    ids = new List<int>();
                    dependenciesFromTo.Add(depId, ids);
                }

                ids.Add(op.Id);
            }
        } // Create the sorted list

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
                    thisIterationIds
                        .Add(item.Key); // Remove all outbound edges
                    List<int> depIds;
                    if (dependenciesFromTo.TryGetValue(item.Key, out depIds))
                    {
                        foreach (var depId in depIds)
                        {
                            dependenciesToFrom[depId].Remove(item.Key);
                        }
                    }
                }
            } // If nothing was found to remove, there's no valid sort.

            if (thisIterationIds.Count == 0)
                return null;
            // Remove the found items from the dictionary and
            // add them to the overall ordering
            foreach (var id in thisIterationIds) dependenciesToFrom.Remove(id);
            overallPartialOrderingIds.AddRange(thisIterationIds);
        }

        return overallPartialOrderingIds;
    }
}