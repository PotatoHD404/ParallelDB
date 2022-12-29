namespace ParallelDB.Queries;

public class DropQuery : IQuery
{
    internal string? table;
    internal bool ifExists;
    
    public DropQuery()
    {
        this.ifExists = false;
    }
    
    public DropQuery Table(string table)
    {
        this.table = table;
        return this;
    }
    
    public DropQuery IfExists()
    {
        ifExists = true;
        return this;
    }
    
    
    
    // toString
    public override string ToString()
    {
        return "DROP TABLE " + (ifExists ? "IF EXISTS " : "") + table;
    }
}