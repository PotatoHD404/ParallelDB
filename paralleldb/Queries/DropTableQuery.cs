namespace ParallelDB.Queries;

public class DropTableQuery : IQuery
{
    private ParallelDb _db;
    internal string? tableName;
    internal bool ifExists;

    public DropTableQuery(ParallelDb db)
    {
        _db = db;
        ifExists = false;
    }

    public DropTableQuery Table(string tableName)
    {
        this.tableName = tableName;
        return this;
    }

    public DropTableQuery IfExists()
    {
        ifExists = true;
        return this;
    }


    // toString
    public override string ToString()
    {
        return "DROP TABLE " + (ifExists ? "IF EXISTS " : "") + tableName;
    }

    public string GetPlan()
    {
        return "";
    }

    // execute
    public bool Execute()
    {
        return _db.Execute(this);
    }
}