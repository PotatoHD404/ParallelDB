using System.Text;
using static ParallelDB.Queries.Globals;

namespace ParallelDB.Queries;

public class InsertQuery : IQuery
{
    private ParallelDb _db;

    internal readonly List<List<dynamic?>> values;
    internal string? into;

    public InsertQuery(ParallelDb db)
    {
        _db = db;
        values = new List<List<dynamic?>>();
    }

    public InsertQuery Into(string table)
    {
        into = table;
        return this;
    }

    public InsertQuery Values(params dynamic?[] values)
    {
        this.values.Add(values.ToList());
        return this;
    }
    
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
                if (values[i][j] is DefaultType)
                    sb.Append("DEFAULT");
                else
                {
                    if (values[i][j] is string)
                    {
                        sb.Append($"'{values[i][j]}'");
                    }
                    else if (values[i][j] is DateTime d)
                    {
                        sb.Append($"'{d.ToString("yyyy-MM-dd HH:mm:ss")}'");
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
        throw new NotImplementedException();
    }

    // execute
    public bool Execute()
    {
        return _db.Execute(this);
    }
}