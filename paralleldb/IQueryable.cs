namespace ParallelDB;

public interface IQueryable<T>
{
    public IQueryable<T> Project(params string[] columns);
    public IQueryable<T> Where(Func<T, bool> predicate);
    public IQueryable<T> Where(Func<T, int, bool> predicate);
    
    public IQueryable<T> Skip(int count);
    
    public IQueryable<T> Take(int count);
    
    public IQueryable<T> Join(IQueryable<T> other, Func<T, T, bool> predicate);
}