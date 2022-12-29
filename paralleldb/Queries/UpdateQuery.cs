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
        // get graphviz graph
        StringBuilder sb = new StringBuilder();
        sb.Append("digraph G {");
        sb.Append("node [shape=box];");
        sb.Append("rankdir=BT;");
        sb.Append("subgraph cluster_0 {");
        sb.Append("label=\"Update\";");
        sb.Append("style=filled;");
        sb.Append("color=lightgrey;");
        sb.Append("node [style=filled,color=white];");
        sb.Append("a0 [label=\"");
        sb.Append(ToString());
        sb.Append("\"];");
        sb.Append("}");
        sb.Append("}");
        return sb.ToString();
    }

    // execute
    public bool Execute()
    {
        return _db.Execute(this);
    }
}