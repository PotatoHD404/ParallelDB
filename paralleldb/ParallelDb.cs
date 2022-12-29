using Antlr4.Runtime;
using ParallelDB.Dependencies;
using ParallelDB.Parse;
using ParallelDB.Queries;
using ParallelDB.Tables;
using SqlParser;

namespace ParallelDB
{
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
            throw new NotImplementedException();
        }

        public bool Execute(CreateQuery createQuery)
        {
            throw new NotImplementedException();
        }

        public bool Execute(DropQuery dropQuery)
        {
            throw new NotImplementedException();
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
}