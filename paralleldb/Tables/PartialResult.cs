namespace ParallelDB.Tables;

public class PartialResult : Queryable<TableRow>
{
    // protected Table? _table;
    // protected IEnumerable<TableRow> _source;

    protected PartialResult(IEnumerable<TableRow> source) : base(source)
    {
        // _table = table;
        // _source = source;
    }

    private PartialResult(IEnumerable<TableRow> source, Table? table) : base(source, table)
    {
        // _table = table;
        // _source = source;
    }

    public override Queryable<TableRow> Project(params string[] columns)
    {
        if (_table is null)
        {
            throw new Exception("Cannot project on a result without a table");
        }

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

    public override Queryable<TableRow> Where(Func<TableRow, bool> predicate)
    {
        return new PartialResult(WhereIterator(predicate), _table);
    }

    private IEnumerable<TableRow> WhereIterator(Func<TableRow, bool> predicate)
    {
        foreach (TableRow element in _source)
        {
            if (predicate(element))
            {
                yield return element;
            }
        }
    }

    public override Queryable<TableRow> Skip(int count)
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

    public override Queryable<TableRow> Take(int count)
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

    public override PartialResult Distinct()
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

    public override Queryable<TableRow> Join(Queryable<TableRow> other, Func<TableRow, TableRow, bool> predicate)
    {
        if (_table is null || other._table is null)
        {
            throw new Exception("Cannot join on a result without a table");
        }

        var newTable = new Table(_table, other._table);
        return new PartialResult(JoinIterator(newTable, other, predicate), newTable);
    }

    private IEnumerable<TableRow> JoinIterator(Table newTable, Queryable<TableRow> other,
        Func<TableRow, TableRow, bool> predicate)
    {
        if (_table is null || other._table is null)
        {
            throw new Exception("Cannot join on a result without a table");
        }

        int index = 0;
        foreach (var row1 in _source)
        {
            foreach (var row2 in other._source)
            {
                if (predicate(row1, row2))
                {
                    var newRow = newTable.NewRow();
                    for (int i = 0; i < _table.ColumnsCount; i++)
                    {
                        newRow[i] = row1[i];
                    }

                    for (int i = 0; i < other._table.ColumnsCount; i++)
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

    public override Queryable<TableRow> Union(Queryable<TableRow> second)
    {
        CheckColumnTypes(second);
        return new PartialResult(UnionIterator(second), _table);
    }

    private void CheckColumnTypes(Queryable<TableRow> second)
    {
        // check if table types are the same
        if (_table is null || second._table is null)
        {
            throw new Exception("Cannot union on a result without a table");
        }

        if (_table.ColumnsCount != second._table.ColumnsCount)
        {
            throw new Exception("Cannot union tables with different column count");
        }

        for (int i = 0; i < _table.ColumnsCount; i++)
        {
            if (_table.ColumnType(i) != second._table.ColumnType(i) || _table.ColumnNullable(i) != second._table.ColumnNullable(i))
            {
                throw new Exception("Cannot union tables with different column types");
            }
        }
    }

    private IEnumerable<TableRow> UnionIterator(Queryable<TableRow> second)
    {
        HashSet<TableRow> set = new();
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

    public override Queryable<TableRow> UnionAll(Queryable<TableRow> second)
    {
        CheckColumnTypes(second);
        return new PartialResult(UnionAllIterator(second), _table);
    }

    private IEnumerable<TableRow> UnionAllIterator(Queryable<TableRow> second)
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

    public override Queryable<TableRow> Intersect(Queryable<TableRow> second)
    {
        CheckColumnTypes(second);
        return new PartialResult(IntersectIterator(second), _table);
    }

    private IEnumerable<TableRow> IntersectIterator(Queryable<TableRow> second)
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

    public override Queryable<TableRow> Except(Queryable<TableRow> second)
    {
        CheckColumnTypes(second);
        return new PartialResult(ExceptIterator(second), _table);
    }

    private IEnumerable<TableRow> ExceptIterator(Queryable<TableRow> second)
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

    public override Table ToTable()
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