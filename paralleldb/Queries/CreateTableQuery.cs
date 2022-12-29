﻿using System.Text;
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
        ifExists = false;
        columns = new Dictionary<string, Column>();
    }
    public CreateTableQuery AddColumn(string name, Column column)
    {
        columns.Add(name, column);
        return this;
    }
    public CreateTableQuery AddColumn(string name, Type type, bool isNullable, bool hasDefault, dynamic? defaultValue)
    {
        columns.Add(name, new Column(name, type, isNullable, hasDefault, defaultValue));
        return this;
    }
    
    public CreateTableQuery AddColumn(string name, Type type, bool isNullable=false, bool hasDefault=false)
    { 
        columns.Add(name, new Column(name, type, isNullable, hasDefault));
        return this;
    }
    
    public CreateTableQuery IfExists()
    {
        ifExists = true;
        return this;
    }
    
    // toString
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("CREATE TABLE ");
        if (ifExists)
            sb.Append("IF NOT EXISTS ");
        sb.Append(tableName);
        sb.Append(" (");
        sb.Append(string.Join(", ", columns.Select(c => c.Value.ToString())));
        sb.Append(")");
        return sb.ToString();
    }
    
}