using System.Linq.Expressions;
using Antlr4.Runtime;
using ParallelDB.Dependencies;
using ParallelDB.Parse;
using ParallelDB.Queries;
using ParallelDB.Tables;
using SqlParser;

namespace ParallelDB;

public class ParallelDb
{
    private TableStorage _tableStorage;
    private DependencyManager _dependencyManager;

    public ParallelDb()
    {
        _tableStorage = new TableStorage();
        _dependencyManager = new DependencyManager();
    }

    public SelectQuery Select() => new SelectQuery(this);
    public InsertQuery Insert() => new InsertQuery(this);
    public UpdateQuery Update() => new UpdateQuery(this);
    public DeleteQuery Delete() => new DeleteQuery(this);
    public CreateQuery Create() => new CreateQuery(this);
    public DropQuery Drop() => new DropQuery(this);

    public Table Execute(SelectQuery selectQuery)
    {
        throw new NotImplementedException();
    }

    public bool Execute(InsertQuery insertQuery)
    {
        throw new NotImplementedException();
    }

    public bool Execute(UpdateQuery updateQuery)
    {
        throw new NotImplementedException();
    }

    public bool Execute(DeleteQuery deleteQuery)
    {
        if (deleteQuery.from is null)
        {
            throw new Exception("Table name is null");
        }
            
        Queryable<TableRow> table = _tableStorage.GetTable(deleteQuery.from)!;
        if (table is null)
        {
            throw new Exception("Table not found");
        }

        var predicate = (IRow row) => deleteQuery.where.All(condition => condition(row));
        if (deleteQuery.where.Count > 0)
        {
            table = table.Where(predicate);
        }
        // combine the expressions with an "and"
        return true;
    }

    public bool Execute(CreateQuery createQuery)
    {
        if (createQuery.tableName is null)
        {
            throw new Exception("Table name is null");
        }

        if (createQuery.columns.Count == 0)
        {
            throw new Exception("No columns specified");
        }

        if (_tableStorage.TableExists(createQuery.tableName))
        {
            if (createQuery.ifNotExists)
            {
                return false;
            }
            throw new Exception("Table already exists");
        }
        var table = new Table(createQuery.tableName);
        foreach (var column in createQuery.columns)
        {
            table.AddColumn(column.Key, column.Value);
        }
        _tableStorage.AddTable(table);
        return true;
    }

    public bool Execute(DropQuery dropQuery)
    {
        if (dropQuery.tableName is null)
        {
            throw new Exception("Table name is null");
        }
            
        if (!_tableStorage.TableExists(dropQuery.tableName))
        {
            if (dropQuery.ifExists)
            {
                return false;
            }
            throw new Exception("Table does not exist");
        }
            
        _tableStorage.RemoveTable(dropQuery.tableName);
        return true;
    }

    private dynamic Execute(IQuery query)
    {
        throw new NotImplementedException();
    }

    public dynamic Execute(string sql)
    {
        return Execute(GetQuery(sql));
    }

    private IQuery GetQuery(string sql)
    {
        var tree = GetTree(sql);
        SqlNodeVisitor sqlNodeVisitor = new();
        throw new NotImplementedException();
        // return sqlNodeVisitor.Visit(tree);
    }

    private static SQLiteParser.ParseContext GetTree(string sql)
    {
        ICharStream stream = CharStreams.fromString(sql);
        var lexer = new SQLiteLexer(stream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new LexerErrorListener());
        var tokens = new CommonTokenStream(lexer);
        var parser = new SQLiteParser(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrorListener());
        return parser.parse();
    }

    public string GetSyntaxTree(string sql)
    {
        var tree = GetTree(sql);
        GraphvizVisitor graphvizVisitor = new();
        graphvizVisitor.Visit(tree);
        return graphvizVisitor.GetGraph();
    }
}