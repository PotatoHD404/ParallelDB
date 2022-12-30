namespace ParallelDB.Queries;

public class DropQuery : IQuery
{
    private ParallelDb _db;
    internal string? tableName;
    internal bool ifExists;

    public DropQuery(ParallelDb db)
    {
        _db = db;
        ifExists = false;
    }

    public DropQuery Table(string tableName)
    {
        this.tableName = tableName;
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