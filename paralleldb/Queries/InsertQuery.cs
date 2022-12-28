using System.Text;

namespace ParallelDB.Queries;

public class InsertQuery : IQuery
{
    internal List<List<dynamic?>> values;
    internal List<List<bool>> @default;
    internal string into;
    
    public InsertQuery(string into)
    {
        this.into = into;
        values = new List<List<dynamic?>>();
        @default = new List<List<bool>>();
    }
    
    public void AddRow(List<dynamic?> values, List<bool> @default)
    {
        this.values.Add(values);
        this.@default.Add(@default);
    }
    
    public void AddRow(List<dynamic?> values)
    {
        this.values.Add(values);
        this.@default.Add(new List<bool>(values.Count));
    }
    
    public void AddRow(params dynamic?[] values)
    {
        this.values.Add(values.ToList());
        this.@default.Add(new List<bool>(values.Length));
    }
    
    public void AddRowDefault(List<bool> @default)
    {
        this.values.Add(new List<dynamic?>(@default.Count));
        this.@default.Add(@default);
    }
    
    public void AddRowDefault(params bool[] @default)
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
                    sb.Append(values[i][j]);
            }
            sb.Append(")");
        }
        return sb.ToString();
    }
}