using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ConsoleTables;
using Parser;
using SqlParser;

namespace Parser;

public class SqlNode
{
}

public class ExpressionNode : SQLiteParser.ExprContext
{
    public ExpressionNode(ParserRuleContext parent, int invokingState) : base(parent, invokingState)
    {
    }

    // Implement computation of the expression
    public bool IsBinary()
    {
        return GetChild(1) != null && GetChild(1) is not SQLiteParser.ExprContext &&
               GetChild(0) is SQLiteParser.ExprContext &&
               GetChild(2) is SQLiteParser.ExprContext;
    }

    public bool IsConstant()
    {
        return literal_value() is not null;
    }
}

public class SourceNode : SqlNode
{
    public readonly SQLiteParser.Table_or_subqueryContext Source;

    public SourceNode(SQLiteParser.Table_or_subqueryContext source)
    {
        Source = source;
    }
}

internal class SelectNode : SqlNode
{
    private readonly List<SqlNode> _columns;
    private readonly SourceNode _from;
    private readonly bool _distinct;
    private readonly ExpressionNode? _where;
    private readonly List<SqlNode> _groupBy;
    private readonly SqlNode? _having;
    private readonly List<SqlNode> _orderBy;
    private readonly SqlNode? _limit;
    private readonly SqlNode? _offset;

    public SelectNode(List<SqlNode> columns, SourceNode from, bool distinct = false, ExpressionNode? where = null,
        List<SqlNode>? groupBy = null, SqlNode? having = null, List<SqlNode>? orderBy = null, SqlNode? limit = null,
        SqlNode? offset = null)
    {
        _columns = columns;
        _from = from;
        _distinct = distinct;
        _where = where;
        _groupBy = groupBy ?? new List<SqlNode>();
        _having = having;
        _orderBy = orderBy ?? new List<SqlNode>();
        _limit = limit;
        _offset = offset;
    }

    public bool Distinct => _distinct;

    public List<SqlNode> Columns => _columns;

    public SqlNode? From => _from;

    // public SqlNode? Where => _where;

    public List<SqlNode> GroupBy => _groupBy;

    public SqlNode? Having => _having;

    public List<SqlNode> OrderBy => _orderBy;

    public SqlNode? Limit => _limit;

    public SqlNode? Offset => _offset;
}

class TableNode : SqlNode
{
    public readonly string Name;

    public TableNode(string name)
    {
        Name = name;
    }
}

class ColumnNode : SqlNode
{
    public readonly string Name;

    public ColumnNode(string name)
    {
        Name = name;
    }
}

class CompoundOperatorNode : SqlNode
{
    public readonly string Operator;

    public CompoundOperatorNode(string @operator)
    {
        Operator = @operator;
    }
}

class CompoundOperator
{
    
}

public class SelectResult : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < 6; i++)
        {
            yield return i * i;
        }
    }
}

// public interface IQueryable<T>
// {
//     public IQueryable<T> Project(params string[] columns);
//     public IQueryable<T> Where(Func<T, bool> predicate);
//     public IQueryable<T> Where(Func<T, int, bool> predicate);
//     
//     public IQueryable<T> Skip(int count);
//     
//     public IQueryable<T> Take(int count);
//     
//     public IQueryable<T> Join(IQueryable<T> other, Func<T, T, bool> predicate);
// }

public class PartialResult
{
    protected Table? _table;
    protected IEnumerable<TableRow> _source;

    protected PartialResult(IEnumerable<TableRow> source)
    {
        _source = source;
    }

    private PartialResult(IEnumerable<TableRow> source, Table? table)
    {
        _source = source;
        _table = table;
    }

    public PartialResult Project(params string[] columns)
    {
        return new PartialResult(ProjectIterator(columns), _table);
    }

    private IEnumerable<TableRow> ProjectIterator(params string[] columns)
    {
        // create new table with only the columns specified
        if (_table is null)
        {
            throw new Exception("Cannot project on a result without a table");
        }

        var newTable = new Table(_table, columns);

        foreach (var row in _source)
        {
            var newRow = newTable.NewRow();
            foreach (var column in columns)
            {
                newRow[column] = row[column];
            }

            yield return newRow;
        }
    }

    public PartialResult Where(Func<TableRow, bool> predicate)
    {
        return Where((row, _) => predicate(row));
    }

    public PartialResult Where(Func<TableRow, int, bool> predicate)
    {
        return new PartialResult(WhereIterator(predicate), _table);
    }

    private IEnumerable<TableRow> WhereIterator(Func<TableRow, int, bool> predicate)
    {
        int index = 0;
        foreach (TableRow element in _source)
        {
            if (predicate(element, index))
            {
                yield return element;
            }

            checked
            {
                ++index;
            }
        }
    }

    public PartialResult Skip(int count)
    {
        return new PartialResult(SkipIterator(count), _table);
    }

    private IEnumerable<TableRow> SkipIterator(int count)
    {
        int i = 0;
        foreach (TableRow element in _source)
        {
            if (i >= count)
            {
                yield return element;
            }

            i++;
        }
    }

    public PartialResult Take(int count)
    {
        return new PartialResult(TakeIterator(count), _table);
    }

    private IEnumerable<TableRow> TakeIterator(int count)
    {
        int i = 0;
        foreach (TableRow element in _source)
        {
            if (i >= count)
            {
                break;
            }

            yield return element;
            i++;
        }
    }

    public PartialResult Distinct()
    {
        return new PartialResult(DistinctIterator(), _table);
    }

    private IEnumerable<TableRow> DistinctIterator()
    {
        HashSet<TableRow> set = new();
        foreach (TableRow element in _source)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    // Join

    public PartialResult Join(PartialResult other, Func<TableRow, TableRow, bool> predicate)
    {
        return Join(other, (row1, row2, _) => predicate(row1, row2));
    }

    public PartialResult Join(PartialResult other, Func<TableRow, TableRow, int, bool> predicate)
    {
        return new PartialResult(JoinIterator(other, predicate), _table);
    }

    private IEnumerable<TableRow> JoinIterator(PartialResult other,
        Func<TableRow, TableRow, int, bool> predicate)
    {
        if (this._table is null || other._table is null)
        {
            throw new Exception("Cannot join on a result without a table");
        }

        int index = 0;

        var newTable = new Table(_table, other._table);
        
        foreach (var row1 in _source)
        {
            foreach (var row2 in other._source)
            {
                if (predicate(row1, row2, index))
                {
                    var newRow = newTable.NewRow();
                    for(int i = 0; i < _table.ColumnsCount; i++)
                    {
                        newRow[i] = row1[i];
                    }
                    
                    for(int i = 0; i < other._table.ColumnsCount; i++)
                    {
                        newRow[i + _table.ColumnsCount] = row2[i];
                    }
                    
                    yield return newRow;
                }

                checked
                {
                    ++index;
                }
            }
        }
        
        
    }

    public PartialResult Union(PartialResult second)
    {
        return Union(second, EqualityComparer<TableRow>.Default);
    }

    public PartialResult Union(PartialResult second, IEqualityComparer<TableRow> comparer)
    {
        return new PartialResult(UnionIterator(second, comparer), _table);
    }

    private IEnumerable<TableRow> UnionIterator(PartialResult second,
        IEqualityComparer<TableRow> comparer)
    {
        HashSet<TableRow> set = new(comparer);
        foreach (TableRow element in _source)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }

        foreach (TableRow element in second._source)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult UnionAll(PartialResult second)
    {
        return new PartialResult(UnionAllIterator(second), _table);
    }

    private IEnumerable<TableRow> UnionAllIterator(PartialResult second)
    {
        foreach (TableRow element in _source)
        {
            yield return element;
        }

        foreach (TableRow element in second._source)
        {
            yield return element;
        }
    }

    public PartialResult Intersect(PartialResult second)
    {
        return new PartialResult(IntersectIterator(second), _table);
    }

    private IEnumerable<TableRow> IntersectIterator(PartialResult second)
    {
        HashSet<TableRow> set = new(second._source);
        foreach (TableRow element in _source)
        {
            if (set.Remove(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult Except(PartialResult second)
    {
        return new PartialResult(ExceptIterator(second), _table);
    }

    private IEnumerable<TableRow> ExceptIterator(PartialResult second)
    {
        HashSet<TableRow> set = new(second._source);
        foreach (TableRow element in _source)
        {
            if (!set.Contains(element))
            {
                yield return element;
            }
        }
    }

    public Table ToTable()
    {
        if (_table is null)
        {
            throw new Exception("Cannot convert to table without a source table");
        }

        var newTable = new Table(_table);

        foreach (var row in _source)
        {
            newTable.AddRow(row);
        }

        return newTable;
    }
}

public class TableRow
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
}

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