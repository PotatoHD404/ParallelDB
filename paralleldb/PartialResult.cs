namespace ParallelDB;

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
        var newTable = new Table(_table, columns);
        return new PartialResult(ProjectIterator(newTable, columns), newTable);
    }

    private IEnumerable<TableRow> ProjectIterator(Table newTable, params string[] columns)
    {
        // create new table with only the columns specified
        if (_table is null)
        {
            throw new Exception("Cannot project on a result without a table");
        }

        foreach (var row in _source)
        {
            var newRow = newTable.NewRow();
            foreach (var column in columns)
            {
                var tmp = row[column];
                newRow[column] = tmp;
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

        newTable.AddRows(_source);

        return newTable;
    }
}