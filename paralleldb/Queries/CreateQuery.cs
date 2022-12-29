using System.Text;
using ParallelDB.Tables;

namespace ParallelDB.Queries;

public class CreateQuery : IQuery
{
    private ParallelDb _db;
    internal string? tableName;
    internal bool ifNotExists;
    internal Dictionary<string, Column> columns;

    public CreateQuery(ParallelDb db)
    {
        _db = db;
        ifNotExists = false;
        columns = new Dictionary<string, Column>();
    }

    public CreateQuery Table(string tableName)
    {
        this.tableName = tableName;
        return this;
    }

    public CreateQuery AddColumn(string name, Column column)
    {
        columns.Add(name, column);
        return this;
    }

    public CreateQuery AddColumn(string name, Type type, bool isNullable, bool hasDefault, dynamic? defaultValue)
    {
        columns.Add(name, new Column(name, type, isNullable, hasDefault, defaultValue));
        return this;
    }

    public CreateQuery AddColumn(string name, Type type, bool isNullable = false, bool hasDefault = false)
    {
        columns.Add(name, new Column(name, type, isNullable, hasDefault));
        return this;
    }

    public CreateQuery IfNotExists()
    {
        ifNotExists = true;
        return this;
    }

    // toString
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("CREATE TABLE ");
        if (ifNotExists)
            sb.Append("IF NOT EXISTS ");
        sb.Append(tableName);
        sb.Append(" (");
        sb.Append(string.Join(", ", columns.Select(c => c.Value.ToString())));
        sb.Append(")");
        return sb.ToString();
    }

    public string GetPlan()
    {
        return "";
    }

    public bool Execute()
    {
        return _db.Execute(this);
    }
}