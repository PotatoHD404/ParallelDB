// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using DependencyExample2;

// Dictionary<int, int?> dict = new();

int ComputeFirst(ConcurrentDictionary<int, dynamic?> _)
{
    Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    return 1;
}

int ComputeSecond(ConcurrentDictionary<int, dynamic?> _)
{
    Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    return 2;
}

int ComputeThird(ConcurrentDictionary<int, dynamic?> dict)
{
    if (dict[1] is null || dict[2] is null)
    {
        throw new Exception("Dependency not met");
    }

    int a = dict[1];
    int b = dict[2];
    return a + b * 10;
}

DependencyManager dm = new DependencyManager();
dm.AddOperation(1, ComputeFirst);
dm.AddOperation(2, ComputeSecond);
dm.AddOperation(3, ComputeThird, 1, 2);
// dm.ExecuteAndWait();
var res = dm.GetResults();

foreach (var item in res)
{
    Console.WriteLine(item);
}