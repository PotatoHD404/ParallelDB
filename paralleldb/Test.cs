namespace ParallelDB;

public class Test
{
    private int _id = 0;
    
    public Func<int> GetId()
    {
        return () => _id;
    }
    
    public Func<int, int> SetId()
    {
        return (id) => _id = id;
    }
}