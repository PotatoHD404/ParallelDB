using ParallelDB.Parse;

namespace ParallelDB.Queries;

public interface IQuery
{
    public string GetPlan();
}