using System.Text;
using ParallelDB.Parse;
using ParallelDB.Queries;
using ParallelDB.Tables;

namespace ParallelDB;

public class UpdateQuery : IQuery
{
    private ParallelDb _db;
    internal string? table;
    internal List<Action<IRow>> set;
    internal List<Func<IRow, bool>> where;

    public UpdateQuery(ParallelDb db)
    {
        _db = db;
        set = new List<Action<IRow>>();
        where = new List<Func<IRow, bool>>();
    }

    public UpdateQuery Table(string table)
    {
        this.table = table;
        return this;
    }

    public UpdateQuery Set(Action<IRow> set)
    {
        this.set.Add(set);
        return this;
    }

    // public UpdateQuery Set(string column, dynamic value)
    // {
    //     set.Add(row => row[column] = value);
    //     return this;
    // }
    public UpdateQuery Where(Func<IRow, bool> where)
    {
        this.where.Add(where);
        return this;
    }

    // toString
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("UPDATE ");
        sb.Append(table);
        sb.Append(" SET ");
        sb.Append(string.Join(", ", set.Select(x => x.ToString())));
        sb.Append(" WHERE ");
        sb.Append(string.Join(" AND ", where.Select(x => x.ToString())));
        return sb.ToString();
    }

    public string GetPlan()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("bgcolor= transparent;");
        sb.AppendLine("rankdir=BT;");
        sb.AppendLine($"\"Scan {this.table}\" -> \"Modify Table {this.table}\"");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // execute
    public bool Execute()
    {
        return _db.Execute(this);
    }
}