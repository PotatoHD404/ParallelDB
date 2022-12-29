namespace ParallelDB.Queries;

public static class Query
{
    public static SelectQuery Select() => new SelectQuery();
    public static InsertQuery Insert() => new InsertQuery();
    public static UpdateQuery Update() => new UpdateQuery();
    public static DeleteQuery Delete() => new DeleteQuery();
    public static CreateQuery Create() => new CreateQuery();
    public static DropQuery Drop() => new DropQuery();
}