﻿namespace Parser;

public static class ObjectDump
{
    public static void Write(TextWriter writer, object obj)
    {
        if (obj == null)
        {
            writer.WriteLine("Object is null");
            return;
        }

        writer.Write("Hash: ");
        writer.WriteLine(obj.GetHashCode());
        writer.Write("Type: ");
        writer.WriteLine(obj.GetType());

        var props = GetProperties(obj);

        if (props.Count > 0)
        {
            writer.WriteLine("-------------------------");
        }

        foreach (var prop in props)
        {
            writer.Write(prop.Key);
            writer.Write(": ");
            writer.WriteLine(prop.Value);
        }
    }

    private static Dictionary<string, string> GetProperties(object obj)
    {
        var props = new Dictionary<string, string>();
        if (obj == null)
            return props;

        var type = obj.GetType();
        foreach (var prop in type.GetProperties())
        {
            var val = prop.GetValue(obj, new object[] { });
            var valStr = val == null ? "" : val.ToString();
            props.Add(prop.Name, valStr);
        }

        return props;
    }
}