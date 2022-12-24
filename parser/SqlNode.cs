using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
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

public class PartialResult<T>
{
    protected IEnumerable<T> _source;

    public PartialResult(IEnumerable<T> source)
    {
        _source = source;
    }

    public PartialResult<T1> Select<T1>(Func<T, T1> selector)
    {
        return Select((row, _) => selector(row));
    }

    public PartialResult<T1> Select<T1>(Func<T, int, T1> selector)
    {
        return new PartialResult<T1>(SelectIterator(_source, selector));
    }

    private static IEnumerable<T1> SelectIterator<T1>(IEnumerable<T> source, Func<T, int, T1> selector)
    {
        int index = 0;
        foreach (T item in source)
        {
            yield return selector(item, index);
            checked
            {
                ++index;
            }
        }
    }

    public PartialResult<T> Where(Func<T, bool> predicate)
    {
        return Where((row, _) => predicate(row));
    }

    public PartialResult<T> Where(Func<T, int, bool> predicate)
    {
        return new PartialResult<T>(WhereIterator(_source, predicate));
    }

    private static IEnumerable<T> WhereIterator(IEnumerable<T> source, Func<T, int, bool> predicate)
    {
        int index = 0;
        foreach (T element in source)
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

    public PartialResult<T> OrderBy<T1>(Func<T, T1> keySelector)
    {
        return OrderBy(keySelector, Comparer<T1>.Default);
    }

    public PartialResult<T> OrderBy<T1>(Func<T, T1> keySelector, IComparer<T1> comparer)
    {
        return new PartialResult<T>(OrderByIterator(_source, keySelector, comparer));
    }


    private static IEnumerable<T> OrderByIterator<T1>(IEnumerable<T> source, Func<T, T1> keySelector,
        IComparer<T1> comparer)
    {
        List<T> list = new();
        foreach (T element in source)
        {
            list.Add(element);
        }

        int[] keys = new int[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            keys[i] = i;
        }

        Array.Sort(keys, (i, j) => comparer.Compare(keySelector(list[i]), keySelector(list[j])));
        foreach (int key in keys)
        {
            yield return list[key];
        }
    }

    public PartialResult<T> OrderByDescending<T1>(Func<T, T1> keySelector)
    {
        return OrderByDescending(keySelector, Comparer<T1>.Default);
    }

    private class ReverseComparer<T1> : IComparer<T1>
    {
        private readonly IComparer<T1> _comparer;

        public ReverseComparer(IComparer<T1> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(T1? x, T1? y)
        {
            return _comparer.Compare(y, x);
        }
    }

    public PartialResult<T> OrderByDescending<T1>(Func<T, T1> keySelector, IComparer<T1> comparer)
    {
        return new PartialResult<T>(OrderByIterator(_source, keySelector, new ReverseComparer<T1>(comparer)));
    }

    public PartialResult<T> OrderByAscending<T1>(Func<T, T1> keySelector)
    {
        return OrderBy(keySelector);
    }

    public PartialResult<T> OrderByAscending<T1>(Func<T, T1> keySelector, IComparer<T1> comparer)
    {
        return OrderBy(keySelector, comparer);
    }

    public PartialResult<T> Skip(int count)
    {
        return new PartialResult<T>(SkipIterator(_source, count));
    }

    private static IEnumerable<T> SkipIterator(IEnumerable<T> source, int count)
    {
        int i = 0;
        foreach (T element in source)
        {
            if (i >= count)
            {
                yield return element;
            }

            i++;
        }
    }

    public PartialResult<T> Take(int count)
    {
        return new PartialResult<T>(TakeIterator(_source, count));
    }

    private static IEnumerable<T> TakeIterator(IEnumerable<T> source, int count)
    {
        int i = 0;
        foreach (T element in source)
        {
            if (i >= count)
            {
                break;
            }

            yield return element;
            i++;
        }
    }

    public PartialResult<T> Distinct()
    {
        return new PartialResult<T>(DistinctIterator(_source));
    }

    private static IEnumerable<T> DistinctIterator(IEnumerable<T> source)
    {
        HashSet<T> set = new();
        foreach (T element in source)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    // Join

    public PartialResult<T3> Join<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T, T1, T3> resultSelector)
    {
        return Join(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<T2>.Default);
    }

    public PartialResult<T3> Join<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T, T1, T3> resultSelector, IEqualityComparer<T2> comparer)
    {
        return new PartialResult<T3>(JoinIterator(_source, inner, outerKeySelector, innerKeySelector, resultSelector,
            comparer));
    }

    private static IEnumerable<T3> JoinIterator<T1, T2, T3>(IEnumerable<T> outer, IEnumerable<T1> inner,
        Func<T, T2> outerKeySelector, Func<T1, T2> innerKeySelector, Func<T, T1, T3> resultSelector,
        IEqualityComparer<T2> comparer)
    {
        Dictionary<T2, T1> dictionary = new();
        foreach (T1 element in inner)
        {
            dictionary.Add(innerKeySelector(element), element);
        }

        foreach (T element in outer)
        {
            if (dictionary.TryGetValue(outerKeySelector(element), out T1? value))
            {
                yield return resultSelector(element, value);
            }
        }
    }

    // LeftJoin

    public PartialResult<T3> LeftJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T, T1?, T3> resultSelector)
    {
        return LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<T2>.Default);
    }

    public PartialResult<T3> LeftJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T, T1?, T3> resultSelector, IEqualityComparer<T2> comparer)
    {
        return new PartialResult<T3>(LeftJoinIterator(_source, inner, outerKeySelector, innerKeySelector,
            resultSelector,
            comparer));
    }

    private static IEnumerable<T3> LeftJoinIterator<T1, T2, T3>(IEnumerable<T> outer, IEnumerable<T1> inner,
        Func<T, T2> outerKeySelector, Func<T1, T2> innerKeySelector, Func<T, T1?, T3> resultSelector,
        IEqualityComparer<T2> comparer)
    {
        Dictionary<T2, T1> dictionary = new();
        foreach (T1 element in inner)
        {
            dictionary.Add(innerKeySelector(element), element);
        }

        foreach (T element in outer)
        {
            dictionary.TryGetValue(outerKeySelector(element), out T1? value);
            yield return resultSelector(element, value);
        }
    }

    // RightJoin

    public PartialResult<T3> RightJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T?, T1, T3> resultSelector)
    {
        return RightJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<T2>.Default);
    }

    public PartialResult<T3> RightJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
        Func<T1, T2> innerKeySelector, Func<T?, T1, T3> resultSelector, IEqualityComparer<T2> comparer)
    {
        return new PartialResult<T3>(RightJoinIterator(_source, inner, outerKeySelector, innerKeySelector,
            resultSelector,
            comparer));
    }

    private static IEnumerable<T3> RightJoinIterator<T1, T2, T3>(IEnumerable<T> outer, IEnumerable<T1> inner,
        Func<T, T2> outerKeySelector, Func<T1, T2> innerKeySelector, Func<T?, T1, T3> resultSelector,
        IEqualityComparer<T2> comparer)
    {
        Dictionary<T2, T> dictionary = new();
        foreach (T element in outer)
        {
            dictionary.Add(outerKeySelector(element), element);
        }

        foreach (T1 element in inner)
        {
            dictionary.TryGetValue(innerKeySelector(element), out T? value);
            yield return resultSelector(value, element);
        }
    }

    // OuterJoin

    // public PartialResult<T3> OuterJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
    //     Func<T1, T2> innerKeySelector, Func<T?, T1?, T3> resultSelector)
    // {
    //     return OuterJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<T2>.Default);
    // }
    //
    // public PartialResult<T3> OuterJoin<T1, T2, T3>(IEnumerable<T1> inner, Func<T, T2> outerKeySelector,
    //     Func<T1, T2> innerKeySelector, Func<T?, T1?, T3> resultSelector, IEqualityComparer<T2> comparer)
    // {
    //     return new PartialResult<T3>(OuterJoinIterator(_source, inner, outerKeySelector, innerKeySelector, resultSelector,
    //         comparer));
    // }
    //
    // private static IEnumerable<T3> OuterJoinIterator<T1, T2, T3>(IEnumerable<T> outer, IEnumerable<T1> inner,
    //     Func<T, T2> outerKeySelector, Func<T1, T2> innerKeySelector, Func<T?, T1?, T3> resultSelector,
    //     IEqualityComparer<T2> comparer)
    // {
    //     Dictionary<T2, T> dictionary1 = new();
    //     foreach (T element in outer)
    //     {
    //         dictionary1.Add(outerKeySelector(element), element);
    //     }
    //
    //     Dictionary<T2, T1> dictionary2 = new();
    //     foreach (T1 element in inner)
    //     {
    //         dictionary2.Add(innerKeySelector(element), element);
    //     }
    //
    //     foreach (T element in outer)
    //     {
    //         T2 key = outerKeySelector(element);
    //         dictionary2.TryGetValue(key, out T1? value);
    //         yield return resultSelector(element, value);
    //         dictionary2.Remove(key);
    //     }
    //
    //     foreach (T1 element in dictionary2.Values)
    //     {
    //         yield return resultSelector(null, element);
    //     }
    //
    //     foreach (T element in dictionary1.Values)
    //     {
    //         yield return resultSelector(element, null);
    //     }
    // }

    // GroupBy

    public PartialResult<IGrouping<T1, T>> GroupBy<T1>(Func<T, T1> keySelector)
    {
        return GroupBy(keySelector, EqualityComparer<T1>.Default);
    }

    public PartialResult<IGrouping<T1, T>> GroupBy<T1>(Func<T, T1> keySelector, IEqualityComparer<T1> comparer)
    {
        return new PartialResult<IGrouping<T1, T>>(GroupByIterator(_source, keySelector, comparer));
    }

    private static IEnumerable<IGrouping<T1, T>> GroupByIterator<T1>(IEnumerable<T> source, Func<T, T1> keySelector,
        IEqualityComparer<T1> comparer)
    {
        Dictionary<T1, List<T>> dictionary = new(comparer);
        foreach (T element in source)
        {
            T1 key = keySelector(element);
            if (!dictionary.TryGetValue(key, out List<T>? list))
            {
                list = new();
                dictionary.Add(key, list);
            }

            list.Add(element);
        }

        foreach (KeyValuePair<T1, List<T>> pair in dictionary)
        {
            yield return new Grouping<T1, T>(pair.Key, pair.Value);
        }
    }

    private class Grouping<T1, T2> : IGrouping<T1, T2>
    {
        private readonly T1 _key;
        private readonly List<T2> _list;

        public Grouping(T1 key, List<T2> list)
        {
            _key = key;
            _list = list;
        }

        public T1 Key => _key;

        public IEnumerator<T2> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // Having

    // public PartialResult<T> Having(Func<T, bool> predicate)
    // {
    //     return new PartialResult<T>(HavingIterator(_source, predicate));
    // }
    //
    // private static IEnumerable<T2> HavingIterator<T1, T2>(IEnumerable<IGrouping<T1, T2>> source, Func<T2, bool> predicate)
    // {
    //     foreach (IGrouping<T1, T2> grouping in source)
    //     {
    //         foreach (T2 element in grouping)
    //         {
    //             if (predicate(element))
    //             {
    //                 yield return (T2) element;
    //             }
    //         }
    //     }
    // }

    public PartialResult<T> Union(IEnumerable<T> second)
    {
        return Union(second, EqualityComparer<T>.Default);
    }

    public PartialResult<T> Union(IEnumerable<T> second, IEqualityComparer<T> comparer)
    {
        return new PartialResult<T>(UnionIterator(_source, second, comparer));
    }

    private static IEnumerable<T> UnionIterator(IEnumerable<T> first, IEnumerable<T> second,
        IEqualityComparer<T> comparer)
    {
        HashSet<T> set = new(comparer);
        foreach (T element in first)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }

        foreach (T element in second)
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult<T> UnionAll(IEnumerable<T> second)
    {
        return new PartialResult<T>(UnionAllIterator(_source, second));
    }

    private static IEnumerable<T> UnionAllIterator(IEnumerable<T> first, IEnumerable<T> second)
    {
        foreach (T element in first)
        {
            yield return element;
        }

        foreach (T element in second)
        {
            yield return element;
        }
    }

    public PartialResult<T> Intersect(IEnumerable<T> second)
    {
        return new PartialResult<T>(IntersectIterator(_source, second));
    }

    private static IEnumerable<T> IntersectIterator(IEnumerable<T> first, IEnumerable<T> second)
    {
        HashSet<T> set = new(second);
        foreach (T element in first)
        {
            if (set.Remove(element))
            {
                yield return element;
            }
        }
    }

    public PartialResult<T> Except(IEnumerable<T> second)
    {
        return new PartialResult<T>(ExceptIterator(_source, second));
    }

    private static IEnumerable<T> ExceptIterator(IEnumerable<T> first, IEnumerable<T> second)
    {
        HashSet<T> set = new(second);
        foreach (T element in first)
        {
            if (!set.Contains(element))
            {
                yield return element;
            }
        }
    }

    public List<T> ToList()
    {
        List<T> list = new();
        foreach (T element in _source)
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

    public object? this[int index]
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
            if (value != null && value.GetType() != _table.ColumnType(index))
            {
                throw new ArgumentException(
                    $"Column {_table.ColumnName(index)} has type {_table.ColumnType(index)} but value {value} has type {value.GetType()}");
            }

            _values[index] = value;
        }
    }
    
    public object? this[string columnName]
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
            if (value != null && value.GetType() != _table.ColumnType(index))
            {
                throw new ArgumentException(
                    $"Column {columnName} has type {_table.ColumnType(index)} but value {value} has type {value.GetType()}");
            }

            _values[index] = value;
        }
    }
    
    public override string ToString()
    {
        return string.Join(", ", _values.Select((value, index) => $"{_table.ColumnName(index)}: {PrettyPrint.Print(value)}"));
    }
}

public static class PrettyPrint
{
    public static string? Print(object? value)
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
                return $"{{{string.Join(", ", dictionary.Keys.Cast<object>().Select(key => $"{Print(key)}: {Print(dictionary[key])}"))}}}";
            case IEnumerable enumerable:
                return $"[{string.Join(", ", enumerable.Cast<object>().Select(Print))}]";
            default:
                return value.ToString();
        }
    }
}
public class Table : PartialResult<TableRow>
{
    private List<TableRow> _rows = new();
    private Dictionary<string, int> _columnIndices = new();
    private List<Type> _columnTypes = new();
    private string? _name;

    public Table(string? name = null) : base(Enumerable.Empty<TableRow>())
    {
        _name = name;
        _source = _rows;
    }

    public string Name => _name;

    public bool IsComputed => _name == null;

    public int ColumnCount => _columnTypes.Count;

    public int RowCount => _rows.Count;

    public string ColumnName(int index)
    {
        return _columnIndices.FirstOrDefault(pair => pair.Value == index).Key;
    }

    public int ColumnIndex(string pairKey)
    {
        return _columnIndices[pairKey];
    }

    public Type ColumnType(int index)
    {
        return _columnTypes[index];
    }

// public Table Select(params string[] columns)
//     {
//         return base.Select(row => new TableRow(this, columns.Select(column => row[column])));
//     }

    public Table AddColumn(string name, Type type)
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

        _columnIndices.Add(name, _columnTypes.Count);
        _columnIndices.Add($"{_name}.{name}", _columnTypes.Count);
        _columnTypes.Add(type);
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
        var row = new TableRow(this, new object?[ColumnCount]);
        _rows.Add(row);
        return row;
    }
}