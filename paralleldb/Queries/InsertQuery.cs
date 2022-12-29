using System.Text;

namespace ParallelDB.Queries;

public class InsertQuery : IQuery
{
    private ParallelDb _db;

    internal List<List<dynamic?>> values;
    internal List<List<bool>> @default;
    internal string? into;

    public InsertQuery(ParallelDb db)
    {
        _db = db;
        values = new List<List<dynamic?>>();
        @default = new List<List<bool>>();
    }

    public InsertQuery Into(string table)
    {
        into = table;
        return this;
    }

    public InsertQuery Values(List<dynamic?> values, List<bool> @default)
    {
        this.values.Add(values);
        this.@default.Add(@default);
        return this;
    }

    public InsertQuery Values(List<dynamic?> values)
    {
        this.values.Add(values);
        @default.Add(new List<bool>(values.Count));
        return this;
    }

    public InsertQuery Values(params dynamic?[] values)
    {
        this.values.Add(values.ToList());
        @default.Add(new bool[values.Length].ToList());
        return this;
    }

    public InsertQuery ValuesDefault(List<bool> @default)
    {
        this.values.Add(new List<dynamic?>(@default.Count));
        this.@default.Add(@default);
        return this;
    }

    public InsertQuery ValuesDefault(params bool[] @default)
    {
        this.values.Add(new List<dynamic?>(@default.Length));
        this.@default.Add(@default.ToList());
        return this;
    }

    // toString 
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("INSERT INTO ");
        sb.Append(into);
        sb.Append(" VALUES ");
        for (int i = 0; i < values.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append("(");
            for (int j = 0; j < values[i].Count; j++)
            {
                if (j > 0)
                    sb.Append(", ");
                if (@default[i][j])
                    sb.Append("DEFAULT");
                else
                {
                    if (values[i][j] is string)
                    {
                        sb.Append($"'{values[i][j]}'");
                    }
                    else if (values[i][j] is DateTime)
                    {
                        sb.Append($"'{((DateTime)values[i][j]).ToString("yyyy-MM-dd HH:mm:ss")}'");
                    }
                    else
                    {
                        sb.Append(values[i][j]);
                    }
                }
            }

            sb.Append(")");
        }

        return sb.ToString();
    }

    public string GetPlan()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("bgcolor= transparent;");
        sb.AppendLine("rankdir=BT;");
        sb.AppendLine($"\"Result\" -> \"Modify Table {this.into}\"");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // execute
    public bool Execute()
    {
        return _db.Execute(this);
    }
}