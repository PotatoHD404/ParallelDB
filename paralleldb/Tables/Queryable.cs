namespace ParallelDB.Tables;

public abstract class Queryable<T>
{
    protected internal Table? _table;
    protected internal IEnumerable<TableRow> _source;
    public Queryable(IEnumerable<TableRow> source, Table? table = null)
    {
        _source = source;
        _table = table;
    }
    public abstract Queryable<T> Project(params string[] columns);
    public abstract Queryable<T> Where(Func<T, bool> predicate);
    public abstract Queryable<T> Skip(int count);
    public abstract Queryable<T> Take(int count);
    public abstract Queryable<T> Join(Queryable<T> other, Func<T, T, bool> predicate);
    
    public abstract Queryable<T> Cartesian(Queryable<T> other);
    // public abstract Queryable<T> InnerJoin(Queryable<T> other, Func<T, T, bool> predicate);
    // public abstract Queryable<T> LeftJoin(Queryable<T> other, Func<T, T, bool> predicate);
    // public abstract Queryable<T> RightJoin(Queryable<T> other, Func<T, T, bool> predicate);
    // public abstract Queryable<T> OuterJoin(Queryable<T> other, Func<T, T, bool> predicate);
    // public abstract Queryable<T> CartesianJoin(Queryable<T> other);
    public abstract Queryable<T> Union(Queryable<T> other);
    public abstract Queryable<T> UnionAll(Queryable<T> other);
    public abstract Queryable<T> Intersect(Queryable<T> other);
    public abstract Queryable<T> Except(Queryable<T> other);
    public abstract Queryable<T> Distinct();
    public abstract Table ToTable();
}