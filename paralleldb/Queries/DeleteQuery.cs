using System.Text;
using ParallelDB.Parse;
using ParallelDB.Tables;

namespace ParallelDB.Queries;

public class DeleteQuery : IQuery
{
    private ParallelDb _db;
    internal string? from;
    internal List<Func<IRow, bool>> where;

    public DeleteQuery(ParallelDb db)
    {
        _db = db;
        where = new List<Func<IRow, bool>>();
    }

    public DeleteQuery Table(string table)
    {
        from = table;
        return this;
    }

    public DeleteQuery Where(Func<IRow, bool> predicate)
    {
        where.Add(predicate);
        return this;
    }

    // toString 
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("DELETE FROM ");
        sb.Append(from);
        if (where.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", this.where.Select(w => w.ToString())));
        }

        return sb.ToString();
    }

    public string GetPlan()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("bgcolor= transparent;");
        sb.AppendLine("rankdir=BT;");
        sb.AppendLine($"\"Scan {this.from}\" -> \"Modify Table {this.from}\"");
        sb.AppendLine("}");
        return sb.ToString();
    }

    public bool Execute()
    {
        return _db.Execute(this);
    }
}