// See https://aka.ms/new-console-template for more information

using DependencyExample;

Console.WriteLine("Hello, World!");

Action oneSecond = () =>
{
    ComputeForOneSecond();

    void ComputeForOneSecond()
    {
        throw new NotImplementedException();
    }
};
DependencyManager dm = new DependencyManager();
dm.AddOperation(1, oneSecond);
dm.AddOperation(2, oneSecond);
dm.AddOperation(3, oneSecond);
dm.AddOperation(4, oneSecond, 1);
dm.AddOperation(5, oneSecond, 1, 2, 3);
dm.AddOperation(6, oneSecond, 3, 4);
dm.AddOperation(7, oneSecond, 5, 6);
dm.AddOperation(8, oneSecond, 5);
dm.Execute();


