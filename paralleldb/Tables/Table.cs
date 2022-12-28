using System.Text;
using System.Text.RegularExpressions;
using ConsoleTables;

namespace ParallelDB.Tables;

public class Table : PartialResult
{
    private readonly List<TableRow> _rows = new();
    private readonly Dictionary<string, int> _columnIndices = new();
    private readonly List<Column> _columns = new();
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
        _columns = new List<Column>(table._columns);
        _rows = new List<TableRow>();
        _table = this;
        _source = _rows;
    }

    public Table(Table table, params string[] columns) : base(Enumerable.Empty<TableRow>())
    {
        // implement select
        _name = table._name;
        _columnIndices = new Dictionary<string, int>();
        _columns = new List<Column>();
        _rows = new List<TableRow>();
        _table = this;
        _source = _rows;
        foreach (string column in columns)
        {
            int index = table.ColumnIndex(column);
            // int index = table.ColumnIndex(column);
            if (column.Contains(".") && !_columnIndices.ContainsKey(column.Split(".")[1]))
            {
                _columnIndices.Add(column.Split(".")[1], _columns.Count);
            }

            _columnIndices.Add(column, _columns.Count);
            if (!column.Contains("."))
            {
                _columnIndices.Add(table._name + "." + column, _columns.Count);
            }

            _columns.Add(table._columns[index]);
        }
    }

    public Table(Table table1, Table table2) : this(table1)
    {
        // check if tables have same names
        if (table1._name == table2._name)
        {
            throw new ArgumentException($"Tables {table1._name} and {table2._name} have same names");
        }

        // join
        foreach (var pair in table2._columnIndices)
        {
            if (!_columnIndices.ContainsKey(pair.Key))
            {
                _columnIndices.Add(pair.Key, _columns.Count);

                if (pair.Key.Contains("."))
                {
                    _columns.Add(table2._columns[pair.Value]);
                }
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

    public bool IsComputed => _name is null;

    public int ColumnsCount => _columns.Count;

    public int RowsCount => _rows.Count;

    public string ColumnName(int index)
    {
        string name = _columnIndices.FirstOrDefault(pair => pair.Value == index).Key;
        if (name is null)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return name;
    }

    public int ColumnIndex(string column)
    {
        // check if column exists
        if (!_columnIndices.ContainsKey(column))
        {
            throw new ArgumentException($"Column {column} does not exist in table {_name}");
        }

        int index = _columnIndices[column];

        if (index == -1)
        {
            throw new ArgumentException($"Column {column} is ambiguous");
        }

        return index;
    }


    public Type ColumnType(int index)
    {
        if (index < 0 || index >= ColumnsCount)
        {
            throw new IndexOutOfRangeException();
        }

        return _columns[index].Type;
    }

    public dynamic? ColumnDefault(int index)
    {
        if (index < 0 || index >= ColumnsCount)
        {
            throw new IndexOutOfRangeException();
        }

        return _columns[index].Default;
    }

    public bool ColumnHasDefault(int index)
    {
        if (index < 0 || index >= ColumnsCount)
        {
            throw new IndexOutOfRangeException();
        }

        return _columns[index].HasDefault;
    }

    public Table AddColumn(string name, Type type, bool nullable = false, bool hasDefault = false)
    {
        dynamic? @default;
        // if nullable type set default to null
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


        return AddColumn(name, type, nullable, hasDefault, @default);
    }

    public Table AddColumn(string name, Type type, bool nullable, bool hasDefault, dynamic? @default)
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

        _columnIndices.Add(name, ColumnsCount);
        if (_name is not null)
            _columnIndices.Add($"{_name}.{name}", ColumnsCount);
        _columns.Add(new Column(type, nullable, hasDefault, @default));
        return this;
    }


    public Table AddRow(Dictionary<string, object?> dictionary)
    {
        _rows.Add(new TableRow(this, dictionary));
        return this;
    }

    public Table AddRow(params object?[] values)
    {
        if (values.Length != ColumnsCount)
        {
            throw new ArgumentException($"Row has {values.Length} values but table has {ColumnsCount} columns");
        }

        _rows.Add(new TableRow(this, values, true));
        return this;
    }

    public Table AddRow(TableRow row)
    {
        for (int i = 0; i < ColumnsCount; i++)
        {
            row.CheckSet(i);
        }

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
            values[i] = ColumnDefault(i);
        }

        var row = new TableRow(this, values);
        // _rows.Add(row);
        return row;
    }

    public string PrettyPrint()
    {
        var sb = new StringBuilder();

        var table = new ConsoleTable(Enumerable.Range(0, ColumnsCount).Select(ColumnName).ToArray());
        foreach (TableRow row in _rows)
        {
            // _values are private, so we need to access via [] operator
            table.AddRow(Enumerable.Range(0, ColumnsCount).Select(i => global::ParallelDB.PrettyPrint.ToString(row[i])).ToArray());
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

        Console.WriteLine(sb.ToString());
    }

    public bool ColumnNullable(int index)
    {
        return _columns[index].IsNullable;
    }

    public List<TableRow> ToRows()
    {
        // Create a deep copy of the rows
        return _rows.Select(row => new TableRow(row)).ToList();
    }
}