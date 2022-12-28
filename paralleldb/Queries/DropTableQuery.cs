namespace ParallelDB.Queries;

public class DropTableQuery : IQuery
{
    internal string table;
    internal bool ifExists;
    
    public DropTableQuery(string table, bool ifExists)
    {
        this.table = table;
        this.ifExists = ifExists;
    }
    
    // toString
    public override string ToString()
    {
        return "DROP TABLE " + (ifExists ? "IF EXISTS " : "") + table;
    }
}