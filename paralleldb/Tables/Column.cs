namespace ParallelDB.Tables;

public class Column
{
    public Type Type { get; }

    // public bool IsPrimaryKey { get; set; }
    // public bool IsForeignKey { get; set; }
    public bool IsNullable { get; }

    public dynamic? Default { get; }

    public bool HasDefault { get; }

    public Column(Type type, bool isNullable, bool hasDefault, dynamic? defaultValue)
    {
        Type = type;
        IsNullable = isNullable;
        HasDefault = hasDefault;
        Default = defaultValue;
    }
}