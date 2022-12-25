using System.Collections;

namespace Parser;

public static class PrettyPrint
{
    public static string? ToString(object? value)
    {
        switch (value)
        {
            case null:
                return "null";
            case string:
                return $"\"{value}\"";
            case char:
                return $"'{value}'";
            case IDictionary dictionary:
                return
                    $"{{{string.Join(", ", dictionary.Keys.Cast<object>().Select(key => $"{ToString(key)}: {ToString(dictionary[key])}"))}}}";
            case IEnumerable enumerable:
                return $"[{string.Join(", ", enumerable.Cast<object>().Select(ToString))}]";
            default:
                return value.ToString();
        }
    }
    
    public static void Print(object? value)
    {
        Console.WriteLine(ToString(value));
    }
}
