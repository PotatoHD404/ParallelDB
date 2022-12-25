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
    private IEnumerable<TableRow> _source;

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
        var newTable = new Table(_table.Name);
        foreach (var column in columns)
        {
            int index = _table.ColumnIndex(column);
            
            newTable.AddColumn(column, _table.ColumnType(index), _table.ColumnDefault(index));
        }
        
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
        return new PartialResult(WhereIterator(_source, predicate), _table);
    }

    private static IEnumerable<TableRow> WhereIterator(IEnumerable<TableRow> source, Func<TableRow, int, bool> predicate)
    {
        int index = 0;
        foreach (TableRow element in source)
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

    // public PartialResult OrderBy(Func<TableRow, TableRow> keySelector)
    // {
    //     return OrderBy(keySelector, Comparer<TableRow>.Default);
    // }
    //
    // public PartialResult OrderBy(Func<TableRow, TableRow> keySelector, IComparer<TableRow> comparer)
    // {
    //     return new PartialResult(OrderByIterator(_source, keySelector, comparer), _table);
    // }
    //
    //
    // private static IEnumerable<TableRow> OrderByIterator(IEnumerable<TableRow> source, Func<TableRow, TableRow> keySelector,
    //     IComparer<TableRow> comparer)
    // {
    //     List<TableRow> list = new();
    //     foreach (TableRow element in source)
    //     {
    //         list.Add(element);
    //     }
    //
    //     int[] keys = new int[list.Count];
    //     for (int i = 0; i < list.Count; i++)
    //     {
    //         keys[i] = i;
    //     }
    //
    //     Array.Sort(keys, (i, j) => comparer.Compare(keySelector(list[i]), keySelector(list[j])));
    //     foreach (int key in keys)
    //     {
    //         yield return list[key];
    //     }
    // }

    // public PartialResult OrderByDescending(Func<TableRow, TableRow> keySelector)
    // {
    //     return OrderByDescending(keySelector, Comparer<TableRow>.Default);
    // }
    //
    // private class ReverseComparer: IComparer<TableRow>
    // {
    //     private readonly IComparer<TableRow> _comparer;
    //
    //     public ReverseComparer(IComparer<TableRow> comparer)
    //     {
    //         _comparer = comparer;
    //     }
    //
    //     public int Compare(TableRow? x, TableRow? y)
    //     {
    //         return _comparer.Compare(y, x);
    //     }
    // }
    //
    // public PartialResult OrderByDescending(Func<TableRow, TableRow> keySelector, IComparer<TableRow> comparer)
    // {
    //     return new PartialResult(OrderByIterator(_source, keySelector, new ReverseComparer(comparer)), _table);
    // }
    //
    // public PartialResult OrderByAscending(Func<TableRow, TableRow> keySelector)
    // {
    //     return OrderBy(keySelector);
    // }
    //
    // public PartialResult OrderByAscending(Func<TableRow, TableRow> keySelector, IComparer<TableRow> comparer)
    // {
    //     return OrderBy(keySelector, comparer);
    // }

    public PartialResult Skip(int count)
    {
        return new PartialResult(SkipIterator(_source, count), _table);
    }

    private static IEnumerable<TableRow> SkipIterator(IEnumerable<TableRow> source, int count)
    {
        int i = 0;
        foreach (TableRow element in source)
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
        return new PartialResult(TakeIterator(_source, count), _table);
    }

    private static IEnumerable<TableRow> TakeIterator(IEnumerable<TableRow> source, int count)
    {
        int i = 0;
        foreach (TableRow element in source)
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
        return new PartialResult(DistinctIterator(_source), _table);
    }

    private static IEnumerable<TableRow> DistinctIterator(IEnumerable<TableRow> source)
    {
        HashSet<TableRow> set = new();
        foreach (TableRow element in source)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    // Join

    public PartialResult Join(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector)
    {
        return Join(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TableRow>.Default);
    }

    public PartialResult Join(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector, IEqualityComparer<TableRow> comparer)
    {
        return new PartialResult(JoinIterator(_source, inner, outerKeySelector, innerKeySelector, resultSelector,
            comparer), _table);
    }

    private static IEnumerable<TableRow> JoinIterator(IEnumerable<TableRow> outer, IEnumerable<TableRow> inner,
        Func<TableRow, TableRow> outerKeySelector, Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector,
        IEqualityComparer<TableRow> comparer)
    {
        Dictionary<TableRow, TableRow> dictionary = new();
        foreach (TableRow element in inner)
        {
            dictionary.Add(innerKeySelector(element), element);
        }

        foreach (TableRow element in outer)
        {
            if (dictionary.TryGetValue(outerKeySelector(element), out TableRow? value))
            {
                yield return resultSelector(element, value);
            }
        }
    }

    // LeftJoin

    public PartialResult LeftJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector)
    {
        return LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TableRow>.Default);
    }

    public PartialResult LeftJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector, IEqualityComparer<TableRow> comparer)
    {
        return new PartialResult(LeftJoinIterator(_source, inner, outerKeySelector, innerKeySelector,
            resultSelector,
            comparer), _table);
    }

    private static IEnumerable<TableRow> LeftJoinIterator(IEnumerable<TableRow> outer, IEnumerable<TableRow> inner,
        Func<TableRow, TableRow> outerKeySelector, Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector,
        IEqualityComparer<TableRow> comparer)
    {
        Dictionary<TableRow, TableRow> dictionary = new();
        foreach (TableRow element in inner)
        {
            dictionary.Add(innerKeySelector(element), element);
        }

        foreach (TableRow element in outer)
        {
            dictionary.TryGetValue(outerKeySelector(element), out TableRow? value);
            yield return resultSelector(element, value);
        }
    }

    // RightJoin

    public PartialResult RightJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector)
    {
        return RightJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TableRow>.Default);
    }

    public PartialResult RightJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
        Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector, IEqualityComparer<TableRow> comparer)
    {
        return new PartialResult(RightJoinIterator(_source, inner, outerKeySelector, innerKeySelector,
            resultSelector,
            comparer), _table);
    }

    private static IEnumerable<TableRow> RightJoinIterator(IEnumerable<TableRow> outer, IEnumerable<TableRow> inner,
        Func<TableRow, TableRow> outerKeySelector, Func<TableRow, TableRow> innerKeySelector, Func<TableRow, TableRow, TableRow> resultSelector,
        IEqualityComparer<TableRow> comparer)
    {
        Dictionary<TableRow, TableRow> dictionary = new();
        foreach (TableRow element in outer)
        {
            dictionary.Add(outerKeySelector(element), element);
        }

        foreach (TableRow element in inner)
        {
            dictionary.TryGetValue(innerKeySelector(element), out TableRow? value);
            yield return resultSelector(value, element);
        }
    }
    // TODO: groupby having joins
    // EDIT: groupby is not possible because it would require a key selector that returns a collection of keys
    // EDIT: having is not needed because it requires groupby to be implemented
    
    // OuterJoin

    // public PartialResult OuterJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
    //     Func<TableRow, TableRow> innerKeySelector, Func<T?, T1?, T3> resultSelector)
    // {
    //     return OuterJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TableRow>.Default);
    // }
    //
    // public PartialResult OuterJoin(IEnumerable<TableRow> inner, Func<TableRow, TableRow> outerKeySelector,
    //     Func<TableRow, TableRow> innerKeySelector, Func<T?, T1?, T3> resultSelector, IEqualityComparer<TableRow> comparer)
    // {
    //     return new PartialResult(OuterJoinIterator(_source, inner, outerKeySelector, innerKeySelector, resultSelector,
    //         comparer));
    // }
    //
    // private static IEnumerable<TableRow> OuterJoinIterator(IEnumerable<TableRow> outer, IEnumerable<TableRow> inner,
    //     Func<TableRow, TableRow> outerKeySelector, Func<TableRow, TableRow> innerKeySelector, Func<T?, T1?, T3> resultSelector,
    //     IEqualityComparer<TableRow> comparer)
    // {
    //     Dictionary<T2, T> dictionary1 = new();
    //     foreach (TableRow element in outer)
    //     {
    //         dictionary1.Add(outerKeySelector(element), element);
    //     }
    //
    //     Dictionary<T2, T1> dictionary2 = new();
    //     foreach (TableRow1 element in inner)
    //     {
    //         dictionary2.Add(innerKeySelector(element), element);
    //     }
    //
    //     foreach (TableRow element in outer)
    //     {
    //         T2 key = outerKeySelector(element);
    //         dictionary2.TryGetValue(key, out T1? value);
    //         yield return resultSelector(element, value);
    //         dictionary2.Remove(key);
    //     }
    //
    //     foreach (TableRow1 element in dictionary2.Values)
    //     {
    //         yield return resultSelector(null, element);
    //     }
    //
    //     foreach (TableRow element in dictionary1.Values)
    //     {
    //         yield return resultSelector(element, null);
    //     }
    // }

    // GroupBy

    // public PartialResult GroupBy(Func<TableRow, TableRow> keySelector)
    // {
    //     return GroupBy(keySelector, EqualityComparer<TableRow>.Default);
    // }
    //
    // public PartialResult GroupBy(Func<TableRow, TableRow> keySelector, IEqualityComparer<TableRow> comparer)
    // {
    //     return new PartialResult(GroupByIterator(_source, keySelector, comparer));
    // }
    //
    // private static IEnumerable<IGrouping<TableRow, TableRow>> GroupByIterator(IEnumerable<TableRow> source, Func<TableRow, TableRow> keySelector,
    //     IEqualityComparer<TableRow> comparer)
    // {
    //     Dictionary<TableRow, List<TableRow>> dictionary = new(comparer);
    //     foreach (TableRow element in source)
    //     {
    //         TableRow key = keySelector(element);
    //         if (!dictionary.TryGetValue(key, out List<TableRow>? list))
    //         {
    //             list = new();
    //             dictionary.Add(key, list);
    //         }
    //
    //         list.Add(element);
    //     }
    //
    //     foreach (KeyValuePair<TableRow, List<TableRow>> pair in dictionary)
    //     {
    //         yield return new Grouping<TableRow, TableRow>(pair.Key, pair.Value);
    //     }
    // }
    //
    // private class Grouping<T1, T2> : IGrouping<T1, T2>
    // {
    //     private readonly T1 _key;
    //     private readonly List<T2> _list;
    //
    //     public Grouping(T1 key, List<T2> list)
    //     {
    //         _key = key;
    //         _list = list;
    //     }
    //
    //     public T1 Key => _key;
    //
    //     public IEnumerator<T2> GetEnumerator()
    //     {
    //         return _list.GetEnumerator();
    //     }
    //
    //     IEnumerator IEnumerable.GetEnumerator()
    //     {
    //         return GetEnumerator();
    //     }
    // }

    // Having

    // public PartialResult Having(Func<TableRow, bool> predicate)
    // {
    //     return new PartialResult(HavingIterator(_source, predicate));
    // }
    //
    // private static IEnumerable<T2> HavingIterator<T1, T2>(IEnumerable<IGrouping<T1, T2>> source, Func<T2, bool> predicate)
    // {
    //     foreach (IGrouping<T1, T2> grouping in source)
    //     {
    //         foreach (TableRow2 element in grouping)
    //         {
    //             if (predicate(element))
    //             {
    //                 yield return (T2) element;
    //             }
    //         }
    //     }
    // }

    public PartialResult Union(IEnumerable<TableRow> second)
    {
        return Union(second, EqualityComparer<TableRow>.Default);
    }

    public PartialResult Union(IEnumerable<TableRow> second, IEqualityComparer<TableRow> comparer)
    {
        return new PartialResult(UnionIterator(_source, second, comparer), _table);
    }

    private static IEnumerable<TableRow> UnionIterator(IEnumerable<TableRow> first, IEnumerable<TableRow> second,
        IEqualityComparer<TableRow> comparer)
    {
        HashSet<TableRow> set = new(comparer);
        foreach (TableRow element in first)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }

        foreach (TableRow element in second)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult UnionAll(IEnumerable<TableRow> second)
    {
        return new PartialResult(UnionAllIterator(_source, second), _table);
    }

    private static IEnumerable<TableRow> UnionAllIterator(IEnumerable<TableRow> first, IEnumerable<TableRow> second)
    {
        foreach (TableRow element in first)
        {
            yield return element;
        }

        foreach (TableRow element in second)
        {
            yield return element;
        }
    }

    public PartialResult Intersect(IEnumerable<TableRow> second)
    {
        return new PartialResult(IntersectIterator(_source, second), _table);
    }

    private static IEnumerable<TableRow> IntersectIterator(IEnumerable<TableRow> first, IEnumerable<TableRow> second)
    {
        HashSet<TableRow> set = new(second);
        foreach (TableRow element in first)
        {
            if (set.Remove(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult Except(IEnumerable<TableRow> second)
    {
        return new PartialResult(ExceptIterator(_source, second), _table);
    }

    private static IEnumerable<TableRow> ExceptIterator(IEnumerable<TableRow> first, IEnumerable<TableRow> second)
    {
        HashSet<TableRow> set = new(second);
        foreach (TableRow element in first)
        {
            if (!set.Contains(element))
            {
                yield return element;
            }
        }
    }

    public List<TableRow> ToList()
    {
        List<TableRow> list = new();
        foreach (TableRow element in _source)
        {
            list.Add(element);
        }

        return list;
    }
}

public class Row : IEnumerable<KeyValuePair<string, object?>>
{
    private readonly Dictionary<string, object?> _dictionary = new();

    public Row()
    {
    }

    public Row(Dictionary<string, object?> dictionary)
    {
        _dictionary = dictionary;
    }

    public object? this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public int Count => _dictionary.Count;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Join(", ", _dictionary.Select(pair => $"{pair.Key}: {pair.Value}"));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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

    public TableRow(Table table, Row row)
    {
        //
        _table = table;
        _values = new object?[row.Count];
        int index;
        foreach (KeyValuePair<string, object?> pair in row)
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
            if(value == null && Nullable.GetUnderlyingType(type) == null) {
                throw new ArgumentException($"Column {value} has type {type} but default value is null");
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            if(value != null && value.GetType() != type) {
                throw new ArgumentException($"Column {index} has type {type} but value {PrettyPrint.ToString(value)} has type {PrettyPrint.ToString(value?.GetType())}");
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
            if(value == null && Nullable.GetUnderlyingType(type) == null) {
                throw new ArgumentException($"Column {PrettyPrint.ToString(columnName)} has type {type} but default value is null");
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            if(value != null && value.GetType() != type) {
                throw new ArgumentException($"Column {PrettyPrint.ToString(columnName)} has type {type} but value {PrettyPrint.ToString(value)} has type {PrettyPrint.ToString(value?.GetType())}");
            }


            _values[index] = value;
        }
    }
    
    public override string ToString()
    {
        return string.Join(", ", _values.Select((value, index) => $"{_table.ColumnName(index)}: {PrettyPrint.ToString(value)}"));
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
                return $"{{{string.Join(", ", dictionary.Keys.Cast<object>().Select(key => $"{ToString(key)}: {ToString(dictionary[key])}"))}}}";
            case IEnumerable enumerable:
                return $"[{string.Join(", ", enumerable.Cast<object>().Select(ToString))}]";
            default:
                return value.ToString();
        }
    }
}
public class Table: PartialResult
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
    }

    public string? Name => _name;
    // add rename method
    public void Rename(string name)
    {
        // check if there is no columns and no rows
        if(_columnIndices.Count == 0 && _rows.Count == 0) {
            _name = name;
        }
        else {
            throw new InvalidOperationException("Cannot rename table with columns or rows");
        }
        
        _name = name;
    }

    public bool IsComputed => _name == null;

    public int ColumnCount => _columnTypes.Count;

    public int RowCount => _rows.Count;

    public string ColumnName(int index)
    {
        return _columnIndices.FirstOrDefault(pair => pair.Value == index).Key;
    }

    public int ColumnIndex(string pairKey)
    { 
        // check if column exists
        if(!_columnIndices.ContainsKey(pairKey)) {
            throw new ArgumentException($"Column {pairKey} does not exist in table {_name}");
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
        if(@default == null && Nullable.GetUnderlyingType(type) == null) {
            throw new ArgumentException($"Column {name} has type {type} but default value is null");
        }
        
        if(@default != null && @default.GetType() != (Nullable.GetUnderlyingType(type) ?? type)) {
            throw new ArgumentException($"Column {name} has type {Nullable.GetUnderlyingType(type)} but default value {@default} has type {@default.GetType()}");
        }

        _columnIndices.Add(name, _columnTypes.Count);
        _columnIndices.Add($"{_name}.{name}", _columnTypes.Count);
        _columnTypes.Add(type);
        _columnDefaults.Add(@default);
        return this;
    }

    public Table AddRow(Row row)
    {
        _rows.Add(new TableRow(this, row));
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

    public Table AddRows(IEnumerable<Row> rows)
    {
        foreach (Row row in rows)
        {
            AddRow(row);
        }

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
        var values = new object?[ColumnCount];
        for(int i = 0; i < ColumnCount; i++)
        {
            values[i] = _columnDefaults[i];
        }
        var row = new TableRow(this, values);
        _rows.Add(row);
        return row;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        var table = new ConsoleTable(Enumerable.Range(0, ColumnCount).Select(ColumnName).ToArray());
        foreach (TableRow row in _rows)
        {
            // _values are private, so we need to access via [] operator
            table.AddRow(Enumerable.Range(0, ColumnCount).Select(i => PrettyPrint.ToString(row[i])).ToArray());
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

