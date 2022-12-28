using ParallelDB.Tables;

namespace ParallelDB.Queries;

public class CreateTableQuery : IQuery
{
    internal string tableName;
    internal bool ifExists;
    internal Dictionary<string, Column> columns;
    public CreateTableQuery(string tableName)
    {
        this.tableName = tableName;
        this.ifExists = false;
        this.columns = new Dictionary<string, Column>();
    }
    public void AddColumn(string name, Column column)
    {
        this.columns.Add(name, column);
    }
    // TODO
    // public void AddColumn(string name, ColumnType type)
    // {
    //     this.columns.Add(name, new Column(type));
    // }
    // public void AddColumn(string name, Type type, bool isNullable, bool hasDefault, dynamic? defaultValue)
    // {
    //     this.columns.Add(name, new Column(type, isNullable, hasDefault, defaultValue));
    // }
    //
    // public void AddColumn(string name, Type type, bool isNullable, bool hasDefault)
    // {
    //     this.columns.Add(name, new Column(type, isNullable, hasDefault));
    // }
    
    
}