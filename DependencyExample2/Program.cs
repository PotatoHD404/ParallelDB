// See https://aka.ms/new-console-template for more information

using DependencyExample2;

Dictionary<int, int?> dict = new();
dict[0] = null;
dict[1] = null;
dict[2] = null;

void ComputeFirst()
{
    Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    dict[0] = 1;
}

void ComputeSecond()
{
    Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    dict[1] = 2;
}

void ComputeThird()
{
    if(dict[0] is null || dict[1] is null)
    {
        throw new Exception("Dependency not met");
    }
    int a = dict[0].Value;
    int b = dict[1].Value;
    dict[2] = a + b;
}

DependencyManager dm = new DependencyManager();
dm.AddOperation(1, ComputeFirst);
dm.AddOperation(2, ComputeSecond);
dm.AddOperation(3, ComputeThird, 1, 2);
dm.ExecuteAndWait();
Console.WriteLine(dict[2]);
// Console.ReadLine();