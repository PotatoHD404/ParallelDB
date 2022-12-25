using System.Text;
using System.Text.RegularExpressions;
using ConsoleTables;

namespace Parser;

public class Table : PartialResult
{
    private readonly List<TableRow> _rows = new();
    private readonly Dictionary<string, int> _columnIndices = new();
    private readonly List<Type> _columnTypes = new();
    private readonly List<dynamic> _columnDefaults = new();
    private string? _name;

    public Table(string? name = null) : base(Enumerable.Empty<TableRow>())
    {
        _name = name;
        _table = this;
        _source = _rows;
    }

    // Table add copy constructor
    public Table(Table table) : base(Enumerable.Empty<TableRow>())
    {
        _name = table._name;
        _columnIndices = new Dictionary<string, int>(table._columnIndices);
        _columnTypes = new List<Type>(table._columnTypes);
        _columnDefaults = new List<dynamic>(table._columnDefaults);
        _rows = new List<TableRow>();
        _table = this;
        _source = _rows;
    }

    public Table(Table table, params string[] columns) : base(Enumerable.Empty<TableRow>())
    {
        // implement select
        _name = table._name;
        _columnIndices = new Dictionary<string, int>();
        _columnTypes = new List<Type>();
        _columnDefaults = new List<dynamic>();
        _rows = new List<TableRow>();
        _table = this;
        _source = _rows;
        
        foreach (string column in columns)
        {
            int index = table.ColumnIndex(column);
            _columnIndices.Add(column, _columnIndices.Count);
            _columnTypes.Add(table.ColumnType(index));
            _columnDefaults.Add(table.ColumnDefault(index));
        }
    }

    public Table(Table table1, Table table2) : this(table1)
    {
        // check if tables have same names
        if (table1._name == table2._name)
        {
            throw new ArgumentException($"Tables {table1._name} and {table2._name} have same names");
        }
        // Inner join
        foreach (var pair in table2._columnIndices)
        {
            if (!_columnIndices.ContainsKey(pair.Key))
            {
                _columnIndices.Add(pair.Key, _columnIndices.Count);
                _columnTypes.Add(table2._columnTypes[pair.Value]);
                _columnDefaults.Add(table2._columnDefaults[pair.Value]);
            }
            else
            {
                _columnIndices[pair.Key] = -1;
            }
        }
    }

    public string? Name => _name;

    // add rename method
    public void Rename(string name)
    {
        // check if there is no columns and no rows
        if (_columnIndices.Count == 0 && _rows.Count == 0)
        {
            _name = name;
        }
        else
        {
            throw new InvalidOperationException("Cannot rename table with columns or rows");
        }

        _name = name;
    }

    public bool IsComputed => _name == null;

    public int ColumnsCount => _columnTypes.Count;

    public int RowCount => _rows.Count;

    public string ColumnName(int index)
    {
        return _columnIndices.FirstOrDefault(pair => pair.Value == index).Key;
    }

    public int ColumnIndex(string pairKey)
    {
        // check if column exists
        if (!_columnIndices.ContainsKey(pairKey))
        {
            throw new ArgumentException($"Column {pairKey} does not exist in table {_name}");
        }
        
        if(_columnIndices[pairKey] == -1)
        {
            throw new ArgumentException($"Column {pairKey} is ambiguous");
        }

        return _columnIndices[pairKey];
    }


    public Type ColumnType(int index)
    {
        if (index < 0 || index >= _columnTypes.Count)
        {
            throw new IndexOutOfRangeException();
        }

        return _columnTypes[index];
    }

    public dynamic ColumnDefault(int index)
    {
        if (index < 0 || index >= _columnTypes.Count)
        {
            throw new IndexOutOfRangeException();
        }

        return _columnDefaults[index];
    }

    public Table AddColumn(string name, Type type)
    {
        return AddColumn(name, type, Activator.CreateInstance(type));
    }

    public Table AddColumn(string name, Type type, object? @default)
    {
        if (_rows.Count > 0)
        {
            throw new InvalidOperationException("Cannot add column to table with rows");
        }

        // check regex
        if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*$"))
        {
            throw new ArgumentException($"Column name {name} is not valid");
        }

        if (_columnIndices.ContainsKey(name))
        {
            throw new ArgumentException($"Column {name} already exists in table {_name}");
        }

        if (@default == null && Nullable.GetUnderlyingType(type) == null)
        {
            throw new ArgumentException($"Column {name} has type {type} but default value is null");
        }

        if (@default != null && @default.GetType() != (Nullable.GetUnderlyingType(type) ?? type))
        {
            throw new ArgumentException(
                $"Column {name} has type {Nullable.GetUnderlyingType(type)} but default value {@default} has type {@default.GetType()}");
        }

        _columnIndices.Add(name, _columnTypes.Count);
        _columnIndices.Add($"{_name}.{name}", _columnTypes.Count);
        _columnTypes.Add(type);
        _columnDefaults.Add(@default);
        return this;
    }
    

    public Table AddRow(Dictionary<string, object?> dictionary)
    {
        _rows.Add(new TableRow(this, dictionary));
        return this;
    }

    public Table AddRow(params object?[] values)
    {
        _rows.Add(new TableRow(this, values));
        return this;
    }

    public Table AddRow(TableRow row)
    {
        _rows.Add(row);
        return this;
    }

    public Table AddRows(IEnumerable<TableRow> rows)
    {
        foreach (TableRow row in rows)
        {
            AddRow(row);
        }

        return this;
    }

    public TableRow NewRow()
    {
        var values = new object?[ColumnsCount];
        for (int i = 0; i < ColumnsCount; i++)
        {
            values[i] = _columnDefaults[i];
        }

        var row = new TableRow(this, values);
        // _rows.Add(row);
        return row;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        var table = new ConsoleTable(Enumerable.Range(0, ColumnsCount).Select(ColumnName).ToArray());
        foreach (TableRow row in _rows)
        {
            // _values are private, so we need to access via [] operator
            table.AddRow(Enumerable.Range(0, ColumnsCount).Select(i => PrettyPrint.ToString(row[i])).ToArray());
        }

        var t = table.ToString();
        int len = t.Split('\n')[0].Length;

        // append table name
        if (_name != null)
        {
            sb.AppendLine(t.Split('\n')[0]);
            sb.Append(' ', (len - _name.Length) / 2 - 1);
            sb.Append(_name);
            sb.AppendLine();
        }

        sb.AppendLine(table.ToString());

        return sb.ToString();
    }
}