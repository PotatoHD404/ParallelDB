namespace ParallelDB.Queries;

public class DropQuery : IQuery
{
    private ParallelDb _db;
    internal string? table;
    internal bool ifExists;

    public DropQuery(ParallelDb db)
    {
        _db = db;
        ifExists = false;
    }

    public DropQuery Table(string tableName)
    {
        table = tableName;
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