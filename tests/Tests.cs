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
    public void WorkTest1()
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
    public void WorkTest1()
    {
        var sql =
            @"SELECT a.doc as 'Врач', b.c 'b' FROM doctors as a WHERE a = b AND c = d OR c = f GROUP by c, d HAVING f = g UNION SELECT 2 INTERSECT SELECT 3 ORDER BY a.name LIMIT 10 OFFSET 10;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void SyntaxErrorTest1()
    {
        var sql = @"SELECT a.doc as 'Врач', b.c 'b' FROM doctors as a`;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void SyntaxErrorTest2()
    {
        var sql = @"SELECT1 a.doc as 'Врач', b.c 'b' FROM doctors as a;";
        Utils.GetGraph(sql);
    }

    [TestMethod]
    public void EmptyTest1()
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
    public void BaseTest1()
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
    public void AddRowErrorTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "a", 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void AddRowErrorTest2()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void WrongTypeRowTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void WrongTypeRowTest2()
    {
        string? a = null;
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow("a", "b");
    }

    [TestMethod]
    public void NullTest1()
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
    public void NullTest2()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(null, "b");
    }

    [TestMethod]
    public void NullTest3()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), true);
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "b");
        table.AddRow(null, "b");

        Assert.IsTrue(table.ColumnsCount == 2);
    }

    [TestMethod]
    public void NullTest4()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string), true);
        table.AddRow(1, "b");
        table.AddRow(2, null);
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NullTest5()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddRow(1, "b");
        table.AddRow(2, null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void DefaultTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, false);
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        var tmp = row["a"];
    }

    [TestMethod]
    public void DefaultTest2()
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
    public void DefaultTest3()
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
    public void DefaultTest4()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        row["a"] = "1";
    }

    [TestMethod]
    public void DefaultTest5()
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
    public void DefaultTest6()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true, null);
        table.AddColumn("b", typeof(string));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void DefaultTest7()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), true, true, null);
        table.AddColumn("b", typeof(string));

        var row = table.NewRow();

        table.AddRow(row);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColNameTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("a", typeof(string));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ColNameTest2()
    {
        var table = new Table();
        table.AddColumn("a.a", typeof(int));
        table.AddColumn("a", typeof(string));
    }

    [TestMethod]
    public void ColNameTest3()
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

    [TestMethod]
    public void DefaultTest8()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true);
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        row["b"] = "1";
        Assert.IsTrue(row["a"] == 0);
        Assert.IsTrue(row["b"] == "1");

        table.AddRow(row);

        Assert.IsTrue(table.RowsCount == 1);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TableImmutabilityTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true);
        table.AddColumn("d", typeof(bool));
    }

    // [TestMethod]
    // [ExpectedException(typeof(ArgumentException))]
    // public void Test24()
    // {
    //     var table = new Table();
    //     table.AddColumn("a", typeof(int));
    //     table.AddColumn("b", typeof(string));
    //     table.AddColumn("c", typeof(bool));
    //
    //     table.AddRow(1, "2", true);
    //     table.AddColumn("d", typeof(bool));
    // }

    [TestMethod]
    public void ColNameTest4()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table = new Table(table, "a", "c");
        Assert.IsTrue(table.ColumnsCount == 2);
        Assert.IsTrue(table.ColumnName(0) == "a");
        Assert.IsTrue(table.ColumnName(1) == "c");
    }

    [TestMethod]
    public void ThreeColTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table = new Table(table);
        Assert.IsTrue(table.ColumnsCount == 3);
    }
}

[TestClass]
public class OperationsTest
{
    [TestMethod]
    public void HashTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        var row1 = table.NewRow();
        row1["a"] = 1;
        row1["b"] = "2";
        row1["c"] = true;
        var row2 = table.NewRow();
        row2["a"] = 1;
        row2["b"] = "2";
        row2["c"] = true;
        Assert.AreNotSame(row1, row2);
        Assert.AreEqual(row1.GetHashCode(), row2.GetHashCode());
    }

    [TestMethod]
    public void HashSetTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        var row = table.NewRow();

        var set = new HashSet<TableRow>();
        set.Add(row);
        row = table.NewRow();
        set.Add(row);
        Assert.AreEqual(1 , set.Count);
    }
    
    [TestMethod]
    public void HashSetTest2()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        var row = table.NewRow();
        row["a"] = 1;
        row["b"] = "2";
        row["c"] = true;
        var set = new HashSet<TableRow>();
        set.Add(row);
        row = table.NewRow();
        row["a"] = 1;
        row["b"] = "2";
        row["c"] = true;
        set.Add(row);
        Assert.AreEqual(1 , set.Count);
    }

    [TestMethod]
    public void ProjectTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        Assert.IsTrue(table.RowsCount == 6);
        var newTable = table.Project("a", "c").ToTable();
        Assert.IsTrue(newTable.RowsCount == 6);
        Assert.IsTrue(newTable.ColumnsCount == 2);
        Assert.IsTrue(newTable.ColumnName(0) == "a");
        Assert.IsTrue(newTable.ColumnName(1) == "c");
    }

    [TestMethod]
    public void JoinTest1()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(bool));
        table2.AddColumn("b", typeof(int));
        table2.AddColumn("c", typeof(string));

        table2.AddRow(true, 1, "2")
            .AddRow(false, 2, "3")
            .AddRow(true, 3, "4");

        var table3 = table1.Join(table2, (el1, el2) => el1["a"] == el2["b"]).ToTable();
        Assert.IsTrue(table3.RowsCount == 3);
        Assert.AreEqual(6, table3.ColumnsCount);
        Assert.AreEqual("table1.a", table3.ColumnName(0));
        Assert.AreEqual("table2.a", table3.ColumnName(4));
    }

    [TestMethod]
    public void WhereTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var newTable = table.Where((el) => el["a"] == 1).ToTable();
        Assert.IsTrue(newTable.RowsCount == 1);
        Assert.IsTrue(newTable.ColumnsCount == 3);
        Assert.IsTrue(newTable.ColumnName(0) == "a");
        Assert.IsTrue(newTable.ColumnName(1) == "b");
        Assert.IsTrue(newTable.ColumnName(2) == "c");
    }

    [TestMethod]
    public void SkipTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var newTable = table.Skip(2).ToTable();
        Assert.IsTrue(newTable.RowsCount == 4);
        Assert.IsTrue(newTable.ColumnsCount == 3);
        Assert.IsTrue(newTable.ColumnName(0) == "a");
        Assert.IsTrue(newTable.ColumnName(1) == "b");
        Assert.IsTrue(newTable.ColumnName(2) == "c");
    }

    [TestMethod]
    public void TakeTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var newTable = table.Take(2).ToTable();
        Assert.IsTrue(newTable.RowsCount == 2);
        Assert.IsTrue(newTable.ColumnsCount == 3);
        Assert.IsTrue(newTable.ColumnName(0) == "a");
        Assert.IsTrue(newTable.ColumnName(1) == "b");
        Assert.IsTrue(newTable.ColumnName(2) == "c");
    }

    [TestMethod]
    public void DistinctTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(5, "6", true);

        var newTable = table.Distinct().ToTable();
        Assert.AreEqual(5, newTable.RowsCount);
        Assert.IsTrue(newTable.ColumnsCount == 3);
        Assert.IsTrue(newTable.ColumnName(0) == "a");
        Assert.IsTrue(newTable.ColumnName(1) == "b");
        Assert.IsTrue(newTable.ColumnName(2) == "c");
    }

    [TestMethod] // TODO: check how this should work
    [ExpectedException(typeof(Exception))]
    public void UnionTest1()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(bool));
        table2.AddColumn("b", typeof(int));
        table2.AddColumn("c", typeof(string));

        table2.AddRow(true, 1, "2")
            .AddRow(false, 2, "3")
            .AddRow(true, 3, "4");

        var table3 = table1.Union(table2).ToTable();
        Assert.IsTrue(table3.RowsCount == 9);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
    }

    [TestMethod]
    public void UnionTest2()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(int));
        table2.AddColumn("b", typeof(string));
        table2.AddColumn("c", typeof(bool));

        table2.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", true);

        var table3 = table1.Union(table2).ToTable();
        Assert.AreEqual(7, table3.RowsCount);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void UnionAllTest1()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(bool));
        table2.AddColumn("b", typeof(int));
        table2.AddColumn("c", typeof(string));

        table2.AddRow(true, 1, "2")
            .AddRow(false, 2, "3")
            .AddRow(true, 3, "4");

        var table3 = table1.UnionAll(table2).ToTable();
        Assert.IsTrue(table3.RowsCount == 9);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
    }

    [TestMethod]
    public void UnionAllTest2()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(int));
        table2.AddColumn("b", typeof(string));
        table2.AddColumn("c", typeof(bool));

        table2.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", true);

        var table3 = table1.UnionAll(table2).ToTable();
        Assert.AreEqual(12, table3.RowsCount);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
    }

    [TestMethod]
    public void IntersectTest1()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "3", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(int));
        table2.AddColumn("b", typeof(string));
        table2.AddColumn("c", typeof(bool));

        table2.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "6", false);

        var table3 = table1.Intersect(table2).ToTable();
        Assert.AreEqual(4, table3.RowsCount);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
    }

    [TestMethod]
    public void ExceptTest1()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table2 = new Table("table2");
        table2.AddColumn("a", typeof(int));
        table2.AddColumn("b", typeof(string));
        table2.AddColumn("c", typeof(bool));

        table2.AddRow(1, "2", true)
            .AddRow(2, "3", false)
            .AddRow(3, "4", true)
            .AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "6", false);

        var table3 = table1.Except(table2).ToTable();
        Assert.AreEqual(1, table3.RowsCount);
        Assert.IsTrue(table3.ColumnsCount == 3);
        Assert.IsTrue(table3.ColumnName(0) == "a");
        Assert.IsTrue(table3.ColumnName(1) == "b");
        Assert.IsTrue(table3.ColumnName(2) == "c");
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