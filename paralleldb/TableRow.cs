namespace Parser;

public class TableRow: IRow
{
    private Table _table;
    private readonly object?[] _values;

    public TableRow(Table table, object?[] values)
    {
        _table = table;
        _values = values;
    }

    public TableRow(Table table, Dictionary<string, object?> dictionary)
    {
        _table = table;
        _values = new object?[dictionary.Values.Count];
        int index;
        foreach (KeyValuePair<string, object?> pair in dictionary)
        {
            index = _table.ColumnIndex(pair.Key);
            if (index == -1)
            {
                throw new ArgumentException($"Column {pair.Key} does not exist in table {_table.Name}");
            }

            if (index >= _values.Length)
            {
                throw new ArgumentException(
                    $"Column {pair.Key} has index {index} which is out of range for table {_table.Name}");
            }

            // check type
            if (pair.Value != null && pair.Value.GetType() != _table.ColumnType(index))
            {
                throw new ArgumentException(
                    $"Column {pair.Key} has type {_table.ColumnType(index)} but value {pair.Value} has type {pair.Value.GetType()}");
            }

            _values[index] = pair.Value;
        }
    }

    public dynamic? this[int index]
    {
        get
        {
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException();
            }

            return _values[index];
        }

        set
        {
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException();
            }

            // check type
            var type = _table.ColumnType(index);
            if (value == null && Nullable.GetUnderlyingType(type) == null)
            {
                throw new ArgumentException($"Column {value} has type {type} but default value is null");
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            if (value != null && value.GetType() != type)
            {
                throw new ArgumentException(
                    $"Column {index} has type {type} but value {PrettyPrint.ToString(value)} has type {PrettyPrint.ToString(value?.GetType())}");
            }

            _values[index] = value;
        }
    }

    public dynamic? this[string columnName]
    {
        get
        {
            int index = _table.ColumnIndex(columnName);
            if (index == -1)
            {
                throw new ArgumentException($"Column {columnName} does not exist in table {_table.Name}");
            }

            return _values[index];
        }

        set
        {
            int index = _table.ColumnIndex(columnName);
            if (index == -1)
            {
                throw new ArgumentException($"Column {columnName} does not exist in table {_table.Name}");
            }

            // check type
            var type = _table.ColumnType(index);
            if (value == null && Nullable.GetUnderlyingType(type) == null)
            {
                throw new ArgumentException(
                    $"Column {PrettyPrint.ToString(columnName)} has type {type} but default value is null");
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            if (value != null && value.GetType() != type)
            {
                throw new ArgumentException(
                    $"Column {PrettyPrint.ToString(columnName)} has type {type} but value {PrettyPrint.ToString(value)} has type {PrettyPrint.ToString(value?.GetType())}");
            }


            _values[index] = value;
        }
    }

    public override int GetHashCode()
    {
        return _values.GetHashCode();
    }

    public override string ToString()
    {
        return string.Join(", ",
            _values.Select((value, index) => $"{_table.ColumnName(index)}: {PrettyPrint.ToString(value)}"));
    }
}