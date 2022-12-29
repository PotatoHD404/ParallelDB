using ParallelDB.Tables;

namespace ParallelDB;

public class TableStorage
{
    private readonly Dictionary<string, Table> _tables = new();

    public void AddTable(Table table)
    {
        if (table.Name is null)
        {
            throw new ArgumentNullException(nameof(table.Name));
        }
        if (_tables.ContainsKey(table.Name))
        {
            throw new ArgumentException($"Table {table.Name} already exists");
        }
        _tables.Add(table.Name, table);
    }
    
    public bool TableExists(string tableName)
    {
        return _tables.ContainsKey(tableName);
    }
    
    public Table? GetTable(string tableName)
    {
        return _tables.TryGetValue(tableName, out var table) ? table : null;
    }

    public void RemoveTable(string tableName)
    {
        _tables.Remove(tableName);
    }
}