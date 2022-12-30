namespace ParallelDB.Tables;

public class Column
{
    public string Name { get; }
    public Type Type { get; }

    // public bool IsPrimaryKey { get; set; }
    // public bool IsForeignKey { get; set; }
    public bool IsNullable { get; }

    public dynamic? Default { get; }

    public bool HasDefault { get; }

    public Column(string name, Type type, bool nullable = true, bool hasDefault = false)
    {
        dynamic? @default;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) || nullable)
        {
            @default = null;
        }
        else if (type == typeof(string))
        {
            @default = "";
        }
        else if (type == typeof(int))
        {
            @default = 0;
        }
        else if (type == typeof(double))
        {
            @default = 0.0;
        }
        else if (type == typeof(bool))
        {
            @default = false;
        }
        else
        {
            try
            {
                @default = Activator.CreateInstance(type);
                if (@default is null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new ArgumentException($"Type {type} is not supported");
            }
        }
        
        nullable = Nullable.GetUnderlyingType(type) is not null || nullable;

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (@default is null && !nullable && hasDefault)
        {
            throw new ArgumentException($"Column {name} has type {type} but default value is null");
        }

        if (@default is not null && @default.GetType() != type && hasDefault)
        {
            throw new ArgumentException(
                $"Column {name} has type {Nullable.GetUnderlyingType(type)} but default value {@default} has type {@default?.GetType()}");
        }
        
        Name = name;
        Type = type;
        IsNullable = nullable;
        HasDefault = hasDefault;
        Default = @default;
    }
    
    public Column(string name,Type type, bool nullable, bool hasDefault, dynamic? @default)
    {

        nullable = Nullable.GetUnderlyingType(type) is not null || nullable;

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (@default is null && !nullable && hasDefault)
        {
            throw new ArgumentException($"Column {name} has type {type} but default value is null");
        }

        if (@default is not null && @default.GetType() != type && hasDefault)
        {
            throw new ArgumentException(
                $"Column {name} has type {type} but default value {@default} has type {@default?.GetType()}");
        }
        
        Name = name;
        Type = type;
        IsNullable = nullable;
        HasDefault = hasDefault;
        Default = @default;
    }
    
    // toString
    public override string ToString()
    {
        return $"{Name} {Type.Name}{(IsNullable ? "" : " NOT NULL")}{(HasDefault ? $" DEFAULT {PrettyPrint.ToString(Default, true)}" : "")}";
    }
    
}