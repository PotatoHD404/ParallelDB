using System.Collections.Concurrent;
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
    public CreateTableQuery Create() => new CreateTableQuery(this);
    public DropTableQuery Drop() => new DropTableQuery(this);

    public Table Execute(SelectQuery selectQuery)
    {
        var dependencyManager = new DependencyManager();
        // Get dependencies and create a dependency graph

        // var table = _tableStorage.GetTable(selectQuery.from[0]);
        int rootId = selectQuery.GetHashCode();
        VisitSelectQuery(selectQuery, dependencyManager);
        var results = dependencyManager.GetResults();
        if (!results.ContainsKey(rootId))
            throw new Exception("No results found for root query");
        var result = results[rootId];
        if (result is null || result is not Table)
        {
            throw new Exception("Result is not a table");
        }

        return result;
    }

    private void VisitSelectQuery(SelectQuery query, DependencyManager dependencyManager)
    {
        if (query.from.Count == 0)
        {
            throw new Exception("No tables specified");
        }

        var deps = query.from
            .Union(query.join.Select(el => el.Item1))
            .Union(query.union)
            .Union(query.unionAll)
            .Union(query.intersect)
            .Union(query.except)
            .OfType<SelectQuery>().ToArray();
        var unvisitedDeps = deps.Where(dep => !dependencyManager.ContainsOperation(dep.GetHashCode()));

        foreach (var dep in unvisitedDeps)
        {
            VisitSelectQuery(dep, dependencyManager);
        }
        var depIds = deps.Select(dep => dep.GetHashCode()).ToArray();
        Func<ConcurrentDictionary<int, dynamic?>, Table> task = dict =>
        {
            
            Table? table = query.from[0] switch
            {
                SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                string tableName => _tableStorage.GetTable(tableName),
                _ => throw new Exception("Invalid table name")
            };
            if (table is null)
            {
                throw new Exception("Table not found");
            }
            table._rwl.AcquireReaderLock(Timeout.Infinite);
            foreach (var id in depIds)
            {
                Table t1 = dict[id] switch
                {
                    Table t => t,
                    _ => throw new Exception("Invalid table")
                };
                t1._rwl.AcquireReaderLock(Timeout.Infinite);
            }
            Queryable<TableRow> queryable = table;

            for (int i = 1; i < query.from.Count; i++)
            {
                Table? joinTable = query.from[i] switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (joinTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.Cartesian(joinTable);
            }

            foreach (var el in query.join)
            {
                Table? joinTable = el.Item1 switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (joinTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.Join(joinTable, (dynamic)el.Item2);
            }

            foreach (var el in query.where)
            {
                queryable = queryable.Where((dynamic)el);
            }

            foreach (var el in query.union)
            {
                Table? unionTable = el switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (unionTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.Union(unionTable);
            }

            foreach (var el in query.unionAll)
            {
                Table? unionTable = el switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (unionTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.UnionAll(unionTable);
            }

            foreach (var el in query.intersect)
            {
                Table? intersectTable = el switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (intersectTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.Intersect(intersectTable);
            }

            foreach (var el in query.except)
            {
                Table? exceptTable = el switch
                {
                    SelectQuery selectQuery => dict[selectQuery.GetHashCode()],
                    string tableName => _tableStorage.GetTable(tableName),
                    _ => throw new Exception("Invalid table name")
                };
                if (exceptTable is null)
                {
                    throw new Exception("Table not found");
                }

                queryable = queryable.Except(exceptTable);
            }

            if (query.limit is { } limit)
            {
                queryable = queryable.Limit(limit);
            }

            if (query.offset is { } offset)
            {
                queryable = queryable.Offset(offset);
            }

            // Project
            if (query.project.Count != 0 && query.project.All(el => el != "*"))
            {
                queryable = queryable.Project(query.project.ToArray());
            }
            var result = queryable.ToTable();
            foreach (var id in depIds)
            {
                Table t1 = dict[id] switch
                {
                    Table t => t,
                    _ => throw new Exception("Invalid table")
                };
                t1._rwl.ReleaseReaderLock();
            }
            table._rwl.ReleaseReaderLock();
            return result;
        };
        dependencyManager.AddOperation(query.GetHashCode(), task, depIds);
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

        if (insertQuery.columns.Count == 0)
        {
            throw new Exception("No columns specified");
        }

        dependencyManager.AddOperation(0, _ =>
        {
            var table = _tableStorage.GetTable(insertQuery.into);
            if (table is null)
            {
                throw new Exception($"Table {insertQuery.into} does not exist");
            }

            table._rwl.AcquireWriterLock(Timeout.Infinite);

            var res = table.Insert(insertQuery.values, insertQuery.columns);
            table._rwl.ReleaseWriterLock();
            return res;
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
            {
                throw new Exception("Table does not exist");
            }

            table._rwl.AcquireWriterLock(Timeout.Infinite);


            Func<IRow, bool> predicate = _ => true;
            Action<IRow> action = row => updateQuery.set.ForEach(x => x(row));
            if (updateQuery.where.Count > 0)
            {
                predicate = row => updateQuery.where.All(condition => condition(row));
            }

            var res = table.Update(action, predicate);
            table._rwl.ReleaseWriterLock();
            return res;
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

            table._rwl.AcquireWriterLock(Timeout.Infinite);
            if (deleteQuery.where.Count > 0)
            {
                bool Predicate(IRow row) => deleteQuery.where.All(condition => condition(row));
                return table.Delete(Predicate);
            }

            var res = table.Truncate();
            table._rwl.ReleaseWriterLock();
            return res;
        });


        // combine the expressions with an "and"
        return dependencyManager.GetResults()[0];
    }

    public bool Execute(CreateTableQuery createTableQuery)
    {
        var dependencyManager = new DependencyManager();
        if (createTableQuery.tableName is null)
        {
            throw new Exception("Table name is null");
        }

        if (createTableQuery.columns.Count == 0)
        {
            throw new Exception("No columns specified");
        }

        if (_tableStorage.TableExists(createTableQuery.tableName))
        {
            if (createTableQuery.ifNotExists)
            {
                return false;
            }

            throw new Exception("Table already exists");
        }

        dependencyManager.AddOperation(0, _ =>
        {
            var table = new Table(createTableQuery.tableName);
            foreach (var column in createTableQuery.columns)
            {
                table.AddColumn(column.Key, column.Value);
            }

            return _tableStorage.AddTable(table);
        });

        return dependencyManager.GetResults()[0];
    }

    public bool Execute(DropTableQuery dropTableQuery)
    {
        var dependencyManager = new DependencyManager();
        if (dropTableQuery.tableName is null)
        {
            throw new Exception("Table name is null");
        }

        if (!_tableStorage.TableExists(dropTableQuery.tableName))
        {
            if (dropTableQuery.ifExists)
            {
                return false;
            }

            throw new Exception("Table does not exist");
        }

        dependencyManager.AddOperation(0, _ => _tableStorage.RemoveTable(dropTableQuery.tableName));
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
            CreateTableQuery createQuery => Execute(createQuery),
            DropTableQuery dropQuery => Execute(dropQuery),
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

    internal Table? GetTable(string tableName)
    {
        return _tableStorage.GetTable(tableName);
    }
}