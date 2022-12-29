using System.Text;
using Parser;

namespace ParallelDB.Queries;

public class DeleteQuery : IQuery
{
    internal string from;
    internal List<Func<IRow, bool>> where;
    
    public DeleteQuery()
    {
        this.where = new List<Func<IRow, bool>>();
    }
    public DeleteQuery From(string from)
    {
        this.from = from;
        return this;
    }
    public DeleteQuery Where(Func<IRow, bool> predicate)
    {
        this.where.Add(predicate);
        return this;
    }
    
    // toString 
       public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("DELETE FROM ");
        sb.Append(this.from);
        if (this.where.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", this.where.Select(w => w.ToString())));
        }
        return sb.ToString();
    }

}