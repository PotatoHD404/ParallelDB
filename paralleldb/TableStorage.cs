using System.Collections.Concurrent;
using ParallelDB.Tables;

namespace ParallelDB;

public class TableStorage
{
    private readonly ConcurrentDictionary<string, Table> _tables = new();

    public bool AddTable(Table table)
    {
        if (table.Name is null)
        {
            throw new ArgumentNullException(nameof(table.Name));
        }
        if (_tables.ContainsKey(table.Name))
        {
            throw new ArgumentException($"Table {table.Name} already exists");
        }
        _tables.TryAdd(table.Name, table);
        return true;
    }
    
    public bool TableExists(string tableName)
    {
        return _tables.ContainsKey(tableName);
    }
    
    public Table? GetTable(string tableName)
    {
        return _tables.TryGetValue(tableName, out var table) ? table : null;
    }

    public bool RemoveTable(string tableName)
    {
        _tables.TryRemove(tableName, out _);
        return true;
    }
}