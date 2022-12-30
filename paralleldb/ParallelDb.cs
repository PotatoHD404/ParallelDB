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
    private QueryVisitor _queryVisitor;
    private GraphvizVisitor _graphvizVisitor;
    public ParallelDb()
    {
        _tableStorage = new TableStorage();
        _queryVisitor = new QueryVisitor(this);
        _graphvizVisitor = new GraphvizVisitor();
    }

    public SelectQuery Select() => new SelectQuery(this);
    public InsertQuery Insert() => new InsertQuery(this);
    public UpdateQuery Update() => new UpdateQuery(this);
    public DeleteQuery Delete() => new DeleteQuery(this);
    public CreateQuery Create() => new CreateQuery(this);
    public DropQuery Drop() => new DropQuery(this);

    public Table Execute(SelectQuery selectQuery)
    {
        var dependencyManager = new DependencyManager();
        if (selectQuery.from.Count == 0)
        {
            throw new Exception("No tables specified");
        }
        // Get dependencies and create a dependency graph
        
        // var table = _tableStorage.GetTable(selectQuery.from[0]);
        throw new NotImplementedException();
    }

    public bool Execute(InsertQuery insertQuery)
    {
        var dependencyManager = new DependencyManager();
        if (insertQuery.into is null)
        {
            throw new Exception("No table specified");
        }

        if (insertQuery.values.Count == 0)
        {
            throw new Exception("No values specified");
        }
        dependencyManager.AddOperation(0, _ =>
        {
            var table = _tableStorage.GetTable(insertQuery.into);
            if (table is null)
            {
                throw new Exception($"Table {insertQuery.into} does not exist");
            }

            return table.Insert(insertQuery.values);
        });
        
        return dependencyManager.GetResults()[0];
    }

    public bool Execute(UpdateQuery updateQuery)
    {
        var dependencyManager = new DependencyManager();
        if (updateQuery.table is null)
            throw new Exception("Table is not specified");
        if (updateQuery.set.Count == 0)
            throw new Exception("No values to update");

        dependencyManager.AddOperation(0, _ =>
        {
            var table = _tableStorage.GetTable(updateQuery.table);
            if (table is null)
                throw new Exception("Table does not exist");


            Func<IRow, bool> predicate = _ => true;
            Action<IRow> action = row => updateQuery.set.ForEach(x => x(row));
            if (updateQuery.where.Count > 0)
            {
                predicate = row => updateQuery.where.All(condition => condition(row));
            }

            return table.Update(action, predicate);
        });

        return dependencyManager.GetResults()[0];
    }

    public bool Execute(DeleteQuery deleteQuery)
    {
        var dependencyManager = new DependencyManager();
        if (deleteQuery.from is null)
        {
            throw new Exception("Table name is null");
        }

        dependencyManager.AddOperation(0, _ =>
        {
            Table? table = _tableStorage.GetTable(deleteQuery.from);
            if (table is null)
            {
                throw new Exception("Table not found");
            }

            if (deleteQuery.where.Count > 0)
            {
                bool Predicate(IRow row) => deleteQuery.where.All(condition => condition(row));
                return table.Delete(Predicate);
            }

            return table.Truncate();
        });


        // combine the expressions with an "and"
        return dependencyManager.GetResults()[0];
    }

    public bool Execute(CreateQuery createQuery)
    {
        var dependencyManager = new DependencyManager();
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

        dependencyManager.AddOperation(0, _ =>
        {
            var table = new Table(createQuery.tableName);
            foreach (var column in createQuery.columns)
            {
                table.AddColumn(column.Key, column.Value);
            }

            return _tableStorage.AddTable(table);
        });

        return dependencyManager.GetResults()[0];
    }

    public bool Execute(DropQuery dropQuery)
    {
        var dependencyManager = new DependencyManager();
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

        dependencyManager.AddOperation(0, _ => _tableStorage.RemoveTable(dropQuery.tableName));
        return dependencyManager.GetResults()[0];
    }

    public dynamic Execute(string sql)
    {
        var query = GetQuery(sql);
        return query switch
        {
            SelectQuery selectQuery => Execute(selectQuery),
            InsertQuery insertQuery => Execute(insertQuery),
            UpdateQuery updateQuery => Execute(updateQuery),
            DeleteQuery deleteQuery => Execute(deleteQuery),
            CreateQuery createQuery => Execute(createQuery),
            DropQuery dropQuery => Execute(dropQuery),
            _ => throw new Exception("Unknown query type")
        };
    }

    public IQuery GetQuery(string sql)
    {
        var tree = GetTree(sql);
        IQuery? res = _queryVisitor.Visit(tree);
        if (res is null)
        {
            throw new Exception("Query is null");
        }
        return res;
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
        _graphvizVisitor.Visit(tree);
        return _graphvizVisitor.GetGraph();
    }
}