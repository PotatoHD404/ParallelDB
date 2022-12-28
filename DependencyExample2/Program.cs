// See https://aka.ms/new-console-template for more information

using DependencyExample2;

static void ComputeForOneSecond()
{
    Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
}

Action oneSecond = () => { ComputeForOneSecond(); };
DependencyManager dm = new DependencyManager();
dm.AddOperation(1, oneSecond);
dm.AddOperation(2, oneSecond,1);
dm.AddOperation(3, oneSecond);
dm.AddOperation(4, oneSecond, 1);
dm.AddOperation(5, oneSecond, 1, 2, 3);
dm.AddOperation(6, oneSecond, 3, 4);
dm.AddOperation(7, oneSecond, 5, 6);
dm.AddOperation(8, oneSecond, 5);
dm.Execute();
Console.ReadLine();