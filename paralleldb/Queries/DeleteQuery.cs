using Parser;

namespace ParallelDB.Queries;

public class DeleteQuery : IQuery
{
    internal string from;
    internal List<Func<IRow, bool>> where;
    
    public DeleteQuery(string table)
    {
        this.from = table;
        this.where = new List<Func<IRow, bool>>();
    }
    
    public DeleteQuery Where(Func<IRow, bool> predicate)
    {
        this.where.Add(predicate);
        return this;
    }
    
}