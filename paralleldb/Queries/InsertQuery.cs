using System.Text;

namespace ParallelDB.Queries;

public class InsertQuery : IQuery
{
    internal List<List<dynamic?>> values;
    internal List<List<bool>> @default;
    internal string? into;
    
    public InsertQuery()
    {
        values = new List<List<dynamic?>>();
        @default = new List<List<bool>>();
    }
    public InsertQuery Into(string table)
    {
        into = table;
        return this;
    }
    public void Values(List<dynamic?> values, List<bool> @default)
    {
        this.values.Add(values);
        this.@default.Add(@default);
    }
    
    public void Values(List<dynamic?> values)
    {
        this.values.Add(values);
        this.@default.Add(new List<bool>(values.Count));
    }
    
    public void Values(params dynamic?[] values)
    {
        this.values.Add(values.ToList());
        this.@default.Add(new bool[values.Length].ToList());
    }
    
    public void ValuesDefault(List<bool> @default)
    {
        this.values.Add(new List<dynamic?>(@default.Count));
        this.@default.Add(@default);
    }
    
    public void ValuesDefault(params bool[] @default)
    {
        this.values.Add(new List<dynamic?>(@default.Length));
        this.@default.Add(@default.ToList());
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
                    } else if (values[i][j] is DateTime)
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
}