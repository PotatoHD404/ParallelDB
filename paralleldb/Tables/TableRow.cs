using ParallelDB.Parse;
using ParallelDB.Queries;

namespace ParallelDB.Tables;

public class TableRow : IRow
{
    private readonly Table _table;
    private readonly dynamic?[] _values;
    private readonly bool[] _isSet;
    private ReaderWriterLock _rwl = new();

    internal TableRow(Table table, dynamic?[] values, bool isSet = false)
    {
        _table = table;
        for (int i = 0; i < values.Length; i++)
        {
            CheckType(i, values[i]);
        }

        _values = values;

        for (int i = 0; i < _values.Length; i++)
        {
            if (_values[i] is DefaultType)
            {
                _values[i] = _table.ColumnDefault(i);
            }
        }

        _isSet = new bool[values.Length];
        if (!isSet) return;

        for (int i = 0; i < _isSet.Length; i++)
        {
            _isSet[i] = true;
        }
    }

    internal TableRow(TableRow row)
    {
        // create deep copy
        _table = row._table;
        _values = new dynamic?[row._values.Length];
        _isSet = new bool[row._isSet.Length];
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = row._values[i];
            _isSet[i] = row._isSet[i];
        }
    }

    internal TableRow(Table table, Dictionary<string, dynamic?> dictionary)
    {
        _table = table;
        _values = new dynamic?[dictionary.Values.Count];
        _isSet = new bool[dictionary.Values.Count];
        foreach (KeyValuePair<string, dynamic?> pair in dictionary)
        {
            var index = _table.ColumnIndex(pair.Key);
            if (index == -1)
            {
                throw new ArgumentException($"Column {pair.Key} does not exist in table {_table.Name}");
            }

            if (index != _values.Length)
            {
                throw new ArgumentException(
                    $"Column {pair.Key} has index {index} which is out of range for table {_table.Name}");
            }

            // check type
            CheckType(index, pair.Value);
            _values[index] = pair.Value is DefaultType ? _table.ColumnDefault(index) : pair.Value;

            _isSet[index] = true;
        }
    }

    void CheckType(int i, dynamic? o)
    {
        var type = _table.ColumnType(i);
        var nullable = _table.ColumnNullable(i);
        if (o is null && !nullable)
        {
            throw new ArgumentException($"Column {_table.ColumnName(i)} is not nullable");
        }

        if (o is DefaultType && !_table.ColumnHasDefault(i))
        {
            throw new ArgumentException($"Column {_table.ColumnName(i)} does not have a default value");
        }

        if (o is not null && o is not DefaultType && o.GetType() != type)
        {
            throw new ArgumentException(
                $"Column {_table.ColumnName(i)} is of type {type} but value is of type {o!.GetType()}");
        }
    }

    public void CheckSet(int i)
    {
        if (!_isSet[i] && !_table.ColumnHasDefault(i))
        {
            throw new ArgumentException($"Column {_table.ColumnName(i)} has not been set");
        }
    }

    public dynamic? this[int index]
    {
        get
        {
            _rwl.AcquireReaderLock(Timeout.Infinite);
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (!_isSet[index] && !_table.ColumnHasDefault(index))
            {
                throw new ArgumentException($"Column {_table.ColumnName(index)} has not been set");
            }
            _rwl.ReleaseReaderLock();
            return _values[index];
        }

        set
        {
            _rwl.AcquireWriterLock(Timeout.Infinite);
            if (index < 0 || index >= _values.Length)
            {
                throw new IndexOutOfRangeException();
            }

            // check type
            CheckType(index, value);

            _values[index] = value is DefaultType ? _table.ColumnDefault(index) : value;
            _isSet[index] = true;
            _rwl.ReleaseWriterLock();
        }
    }

    public dynamic? this[string columnName]
    {
        get
        {
            _rwl.AcquireReaderLock(Timeout.Infinite);
            int index = _table.ColumnIndex(columnName);
            if (index == -1)
            {
                throw new ArgumentException($"Column {columnName} does not exist in table {_table.Name}");
            }

            if (!_isSet[index] && !_table.ColumnHasDefault(index))
            {
                throw new ArgumentException($"Column {columnName} has not been set");
            }
            var value = _values[index];
            _rwl.ReleaseReaderLock();
            return value;
        }

        set
        {
            _rwl.AcquireWriterLock(Timeout.Infinite);
            int index = _table.ColumnIndex(columnName);
            if (index == -1)
            {
                throw new ArgumentException($"Column {columnName} does not exist in table {_table.Name}");
            }

            // check type
            CheckType(index, value);

            _values[index] = value is DefaultType ? _table.ColumnDefault(index) : value;

            _isSet[index] = true;
            _rwl.ReleaseWriterLock();
        }
    }

    public override int GetHashCode()
    {
        // get same hash code for rows with same values
        int hash = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            if (_isSet[i])
            {
                hash ^= _values[i] is not null ? _values[i]!.GetHashCode() : 0;
            }
        }

        return hash;
    }

    public override bool Equals(dynamic? obj)
    {
        // if parameter is null return false.
        if (obj is null)
        {
            return false;
        }

        // if parameter cannot be cast to TableRow return false.
        if (!(obj is TableRow row))
        {
            return false;
        }

        // return true if the fields match:
        return _values.SequenceEqual(row._values);
    }

    public override string ToString()
    {
        return string.Join(", ",
            _values.Select((value, index) => $"{_table.ColumnName(index)}: {PrettyPrint.ToString(value)}"));
    }
    
    // internal dynamic?[] Values => _values;
}