namespace ParallelDB;

public class Column
{
    public string Name { get; }

    public Type Type { get; }

    // public bool IsPrimaryKey { get; set; }
    // public bool IsForeignKey { get; set; }
    public bool IsNullable { get; }

    public dynamic? Default { get; }

    public bool HasDefault { get; }

    public Column(string name, Type type, bool isNullable, bool hasDefault, dynamic? defaultValue)
    {
        Name = name;
        Type = type;
        IsNullable = isNullable;
        HasDefault = hasDefault;
        Default = defaultValue;
    }
}