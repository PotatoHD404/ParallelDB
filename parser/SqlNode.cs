namespace Parser;

public class SqlNode
{
    
}

public class SelectNode : SqlNode
{
    public SelectNode()
    {
        this.Columns = new List<ColumnNode>();
        this.From = new TableNode();
        this.Where = new List<WhereNode>();
    }
    
    public List<ColumnNode> Columns { get; set; }
    public TableNode From { get; set; }
    public List<WhereNode> Where { get; set; }
    
    public override string ToString()
    {
        return "SELECT " + string.Join(", ", this.Columns) + " FROM " + this.From + " WHERE " + string.Join(" AND ", this.Where);
    }
}

public class ColumnNode : SqlNode
{
    public string Name { get; set; }
}

public class TableNode : SqlNode
{
    public string Name { get; set; }
}

public class WhereNode : SqlNode
{
    public string Column { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
}

