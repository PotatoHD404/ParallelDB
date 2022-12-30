using System.Collections;

namespace ParallelDB;

public static class PrettyPrint
{
    public static string? ToString(object? value, bool sql = false)
    {
        if (value is string && sql)
            return $"'{value}'";
        return value switch
        {
            null => "null",
            char => $"'{value}'",
            string => $"\"{value}\"",
            IDictionary dictionary =>
                $"{{{string.Join(", ", dictionary.Keys.Cast<object>().Select(key => $"{ToString(key, sql)}: {ToString(dictionary[key], sql)}"))}}}",
            IEnumerable enumerable =>
                $"[{string.Join(", ", enumerable.Cast<object>().Select((el) => ToString(el, sql)))}]",
            _ => value.ToString()
        };
    }

    public static void Print(object? value, bool sql = false)
    {
        Console.WriteLine(ToString(value, sql));
    }
}