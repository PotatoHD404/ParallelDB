using Antlr4.Runtime;
using ParallelDB;
using Parser;
using SqlParser;

namespace Tests;

public static class Utils
{
    public static string ReadFile()
    {
        // Read the file as one string.
        // use Path.Combine to get the full path to the file
        return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "query_examples",
            "example.sql"));
    }

    public static string GetGraph(string sql)
    {
        ICharStream stream = CharStreams.fromString(sql);
        var lexer = new SQLiteLexer(stream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new LexerErrorListener());
        var tokens = new CommonTokenStream(lexer);
        var parser = new SQLiteParser(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrorListener());
        var tree = parser.parse();
        GraphvizVisitor graphvizVisitor = new();
        graphvizVisitor.Visit(tree);
        return graphvizVisitor.GetGraph();
    }
}

[TestClass]
public class GraphVizTest
{
    [TestMethod]
    public void Test1()
    {
        var sql = Utils.ReadFile();
        var graph = Utils.GetGraph(sql);
        // check if graph is correct
        Assert.IsTrue(graph.Contains("digraph"));
        Assert.IsTrue(graph.Contains("doctors"));
        Assert.IsTrue(graph.Contains("Врач"));
        Assert.IsTrue(graph.Contains("HAVING"));
        Assert.IsTrue(graph.Contains("UNION"));
    }
}

[TestClass]
public class ParserTest
{
    [TestMethod]
    public void Test1()
    {
        var sql =
            @"SELECT a.doc as 'Врач', b.c 'b' FROM doctors as a WHERE a = b AND c = d OR c = f GROUP by c, d HAVING f = g UNION SELECT 2 INTERSECT SELECT 3 ORDER BY a.name LIMIT 10 OFFSET 10;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void Test2()
    {
        var sql = @"SELECT a.doc as 'Врач', b.c 'b' FROM doctors as a`;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void Test3()
    {
        var sql = @"SELECT1 a.doc as 'Врач', b.c 'b' FROM doctors as a;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    public void Test4()
    {
        var sql = @"";
        Utils.GetGraph(sql);
    }
}

[TestClass]
public class SqlVisitorTest
{
    [TestMethod]
    public void Test1()
    {
        // TODO: implement
    }
}

[TestClass]
public class TableTest
{
    [TestMethod]
    public void Test1()
    {
        var table = new Table();
        Assert.IsTrue(table.ColumnsCount == 0);
        Assert.IsTrue(table.RowsCount == 0);

        table.AddColumn("a", typeof(int));
        Assert.IsTrue(table.ColumnsCount == 1);
        Assert.IsTrue(table.RowsCount == 0);

        table.AddColumn("b", typeof(string));
        Assert.IsTrue(table.ColumnsCount == 2);
        Assert.IsTrue(table.RowsCount == 0);

        table.AddRow(1, "a");
        Assert.IsTrue(table.ColumnsCount == 2);
        Assert.IsTrue(table.RowsCount == 1);

        table.AddRow(2, "b");
        Assert.IsTrue(table.ColumnsCount == 2);
        Assert.IsTrue(table.RowsCount == 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test2()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "a", 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test3()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test4()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test5()
    {
        string? a = null;
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow("a", "b");
    }

    [TestMethod]
    public void Test6()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int?));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "b");
        table.AddRow(null, "b");
        
        Assert.IsTrue(table.ColumnsCount == 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test7()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(null, "b");
    }

    [TestMethod]
    public void Test8()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), true);
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "b");
        table.AddRow(null, "b");

        Assert.IsTrue(table.ColumnsCount == 2);
    }

    [TestMethod]
    public void Test9()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string), true);
        table.AddRow(1, "b");
        table.AddRow(2, null);
    }
    
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test10()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "b");
        table.AddRow(2, null);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test11()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, false);
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        var tmp = row["a"];
    }
    
    [TestMethod]
    public void Test12()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true);
        table.AddColumn("b", typeof(string), false, true);
        var row = table.NewRow();

        var tmp = row["a"];
        Assert.IsTrue(tmp == 0);
        
        table.AddRow(row);
        Assert.IsTrue(table.RowsCount == 1);
    }
    
    
    [TestMethod]
    public void Test13()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true);
        table.AddColumn("b", typeof(string), false, true);
        var row = table.NewRow();

        row["a"] = 1;
        Assert.IsTrue(row["a"] == 1);
        
        table.AddRow(row);
        Assert.IsTrue(table.RowsCount == 1);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test14()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        row["a"] = "1";
    }
    
    [TestMethod]
    public void Test15()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true, 2);
        table.AddColumn("b", typeof(string), false, true);
        var row = table.NewRow();

        Assert.IsTrue(row["a"] == 2);
        
        table.AddRow(row);
        Assert.IsTrue(table.RowsCount == 1);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test16()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true, null);
        table.AddColumn("b", typeof(string));
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test17()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), true, true, null);
        table.AddColumn("b", typeof(string));
        
        var row = table.NewRow();
        
        table.AddRow(row);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test18()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), true, true, null);
        table.AddColumn("b", typeof(string));
        
        var row = table.NewRow();
        
        table.AddRow(row);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test19()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("a", typeof(string));

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test20()
    {
        var table = new Table();
        table.AddColumn("a.a", typeof(int));
        table.AddColumn("a", typeof(string));

    }
    
    [TestMethod]
    public void Test21()
    {
        var table = new Table("table");
        table.AddColumn("a", typeof(int), true, true, 2);
        table.AddColumn("b", typeof(string), true, true, "1");
        
        var row = table.NewRow();
        Assert.IsTrue(row["a"] == 2);
        Assert.IsTrue(row["b"] == "1");
        Assert.IsTrue(row["table.a"] == 2);
        Assert.IsTrue(row["table.b"] == "1");
    }
}

[TestClass]
public class QueryPlannerTest
{
    [TestMethod]
    public void Test1()
    {
        // TODO: implement
    }
}