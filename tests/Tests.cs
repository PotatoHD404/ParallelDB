using System.Collections.Concurrent;
using Antlr4.Runtime;
using ParallelDB;
using ParallelDB.Dependencies;
using ParallelDB.Parse;
using ParallelDB.Queries;
using ParallelDB.Tables;
using SqlParser;
using static ParallelDB.Queries.Globals;

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
}

[TestClass]
public class GraphVizTest
{
    [TestMethod]
    public void WorkTest1()
    {
        var sql = Utils.ReadFile();
        var db = new ParallelDb();
        var graph = db.GetSyntaxTree(sql);
        // check if graph is correct
        Assert.AreEqual(true, graph.Contains("digraph"));
        Assert.AreEqual(true, graph.Contains("doctors"));
        Assert.AreEqual(true, graph.Contains("Врач"));
        Assert.AreEqual(true, graph.Contains("HAVING"));
        Assert.AreEqual(true, graph.Contains("UNION"));
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
        var db = new ParallelDb();
        db.GetSyntaxTree(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void SyntaxErrorTest1()
    {
        var sql = @"SELECT a.doc as 'Врач', b.c 'b' FROM doctors as a`;";
        var db = new ParallelDb();
        db.GetSyntaxTree(sql);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void SyntaxErrorTest2()
    {
        var sql = @"SELECT1 a.doc as 'Врач', b.c 'b' FROM doctors as a;";
        var db = new ParallelDb();
        db.GetSyntaxTree(sql);
    }

    [TestMethod]
    public void EmptyTest1()
    {
        var sql = @"";
        var db = new ParallelDb();
        db.GetSyntaxTree(sql);
    }
}

[TestClass]
public class TableTest
{
    [TestMethod]
    public void BaseTest1()
    {
        var table = new Table();
        Assert.AreEqual(0, table.ColumnsCount);
        Assert.AreEqual(0, table.RowsCount);

        table.AddColumn("a", typeof(int));
        Assert.AreEqual(1, table.ColumnsCount);
        Assert.AreEqual(0, table.RowsCount);

        table.AddColumn("b", typeof(string));
        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual(0, table.RowsCount);

        table.AddRow(1, "a");
        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual(1, table.RowsCount);

        table.AddRow(2, "b");
        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual(2, table.RowsCount);
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

        Assert.AreEqual(2, table.ColumnsCount);
    }

    [TestMethod]
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

        Assert.AreEqual(2, table.ColumnsCount);
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
        Assert.AreEqual(0, tmp);

        table.AddRow(row);
        Assert.AreEqual(1, table.RowsCount);
    }


    [TestMethod]
    public void DefaultTest3()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true);
        table.AddColumn("b", typeof(string), false, true);
        var row = table.NewRow();

        row["a"] = 1;
        Assert.AreEqual(1, row["a"]);

        table.AddRow(row);
        Assert.AreEqual(1, table.RowsCount);
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

        Assert.AreEqual(2, row["a"]);

        table.AddRow(row);
        Assert.AreEqual(1, table.RowsCount);
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
        Assert.AreEqual(2, row["a"]);
        Assert.AreEqual("1", row["b"]);
        Assert.AreEqual(2, row["table.a"]);
        Assert.AreEqual("1", row["table.b"]);
    }

    [TestMethod]
    public void DefaultTest8()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int), false, true);
        table.AddColumn("b", typeof(string));
        var row = table.NewRow();

        row["b"] = "1";
        Assert.AreEqual(0, row["a"]);
        Assert.AreEqual("1", row["b"]);

        table.AddRow(row);

        Assert.AreEqual(1, table.RowsCount);
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
        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual("a", table.ColumnName(0));
        Assert.AreEqual("c", table.ColumnName(1));
    }

    [TestMethod]
    public void ThreeColTest1()
    {
        var table = new Table();
        table.AddColumn("a", typeof(int));
        table.AddColumn("b", typeof(string));
        table.AddColumn("c", typeof(bool));

        table = new Table(table);
        Assert.AreEqual(3, table.ColumnsCount);
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
        Assert.AreEqual(1, set.Count);
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
        Assert.AreEqual(1, set.Count);
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

        Assert.AreEqual(6, table.RowsCount);
        var newTable = table.Project("a", "c").ToTable();
        Assert.AreEqual(6, newTable.RowsCount);
        Assert.AreEqual(2, newTable.ColumnsCount);
        Assert.AreEqual("a", newTable.ColumnName(0));
        Assert.AreEqual("c", newTable.ColumnName(1));
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
        Assert.AreEqual(3, table3.RowsCount);
        Assert.AreEqual(6, table3.ColumnsCount);
        Assert.AreEqual("table1.a", table3.ColumnName(0));
        Assert.AreEqual("table1.b", table3.ColumnName(1));
        Assert.AreEqual("table1.c", table3.ColumnName(2));
        Assert.AreEqual("table2.a", table3.ColumnName(3));
        Assert.AreEqual("table2.b", table3.ColumnName(4));
        Assert.AreEqual("table2.c", table3.ColumnName(5));
        Assert.AreEqual(5, table3.ColumnIndex("table2.c"));
        Assert.AreEqual(2, table3.ColumnIndex("table1.c"));
        var rows = table3.ToRows();
        Console.WriteLine(rows[0][1]);
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void JoinTest2()
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
        Assert.AreEqual(-1, table3.ColumnIndex("c"));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void JoinTest3()
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
        Assert.AreEqual(-1, table3.ColumnIndex("d"));
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
        Assert.AreEqual(1, newTable.RowsCount);
        Assert.AreEqual(3, newTable.ColumnsCount);
        Assert.AreEqual("a", newTable.ColumnName(0));
        Assert.AreEqual("b", newTable.ColumnName(1));
        Assert.AreEqual("c", newTable.ColumnName(2));
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
        Assert.AreEqual(4, newTable.RowsCount);
        Assert.AreEqual(3, newTable.ColumnsCount);
        Assert.AreEqual("a", newTable.ColumnName(0));
        Assert.AreEqual("b", newTable.ColumnName(1));
        Assert.AreEqual("c", newTable.ColumnName(2));
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
        Assert.AreEqual(2, newTable.RowsCount);
        Assert.AreEqual(3, newTable.ColumnsCount);
        Assert.AreEqual("a", newTable.ColumnName(0));
        Assert.AreEqual("b", newTable.ColumnName(1));
        Assert.AreEqual("c", newTable.ColumnName(2));
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
        Assert.AreEqual(3, newTable.ColumnsCount);
        Assert.AreEqual("a", newTable.ColumnName(0));
        Assert.AreEqual("b", newTable.ColumnName(1));
        Assert.AreEqual("c", newTable.ColumnName(2));
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
        Assert.AreEqual(9, table3.RowsCount);
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
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
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
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
        Assert.AreEqual(9, table3.RowsCount);
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
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
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
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
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
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
        Assert.AreEqual(3, table3.ColumnsCount);
        Assert.AreEqual("a", table3.ColumnName(0));
        Assert.AreEqual("b", table3.ColumnName(1));
        Assert.AreEqual("c", table3.ColumnName(2));
    }

    [TestMethod]
    public void MultipleOperationsTest1()
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

        var table3 = table1.Join(table2, (el1, el2) => el1["a"] == el2["b"]).Where((el) => el["table1.a"] == 1)
            .ToTable();
        var rows = table3.ToRows();
        Assert.AreEqual(1, table3.RowsCount);
        Assert.AreEqual(6, table3.ColumnsCount);
        Assert.AreEqual(1, rows[0][0]);
    }

    [TestMethod]
    public void MultipleOperationsTest2()
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

        table2.AddRow(4, "5", false)
            .AddRow(5, "6", true)
            .AddRow(6, "7", false);

        var table3 = new Table("table3");
        table3.AddColumn("a", typeof(int));
        table3.AddColumn("b", typeof(string));
        table3.AddColumn("c", typeof(bool));

        table3.AddRow(2, "3", false);

        var table4 = table1.Except(table2).Intersect(table3).ToTable();
        var rows = table4.ToRows();
        Assert.AreEqual(1, table4.RowsCount);
        Assert.AreEqual(3, table4.ColumnsCount);
        Assert.AreEqual(2, rows[0][0]);
        Assert.AreEqual("3", rows[0][1]);
        Assert.AreEqual(false, rows[0][2]);
    }

    [TestMethod]
    public void MultipleOperationsTest3()
    {
        var table1 = new Table("table1");
        table1.AddColumn("a", typeof(int));
        table1.AddColumn("b", typeof(string));
        table1.AddColumn("c", typeof(bool));

        table1.AddRow(1, "e", false)
            .AddRow(2, "e", false)
            .AddRow(3, "e", false)
            .AddRow(4, "e", false)
            .AddRow(5, "e", true)
            .AddRow(6, "e", false);

        var table2 = table1.Take(3).Project("b", "c").Distinct().ToTable();
        var rows = table2.ToRows();
        Assert.AreEqual(1, table2.RowsCount);
        Assert.AreEqual(2, table2.ColumnsCount);
        Assert.AreEqual("e", rows[0][0]);
        Assert.AreEqual(false, rows[0][1]);
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

    [TestClass]
    public class QueryTest
    {
        [TestMethod]
        public void SelectQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Select().From("table1").Intersect("table2").Take(3);
            Assert.AreEqual("SELECT * FROM table1 INTERSECT table2 LIMIT 3", query.ToString());
        }

        [TestMethod]
        public void SelectQueryTest2()
        {
            var db = new ParallelDb();
            var query = db.Select().From(db.Select().From("table2")).Skip(3);
            Assert.AreEqual("SELECT * FROM (SELECT * FROM table2) OFFSET 3", query.ToString());
        }

        [TestMethod]
        public void SelectQueryTest3()
        {
            var db = new ParallelDb();
            var query = db.Select().From("table1").From(db.Select().From("table2"));
            Assert.AreEqual("SELECT * FROM table1, (SELECT * FROM table2)", query.ToString());
        }

        [TestMethod]
        public void SelectQueryTest4()
        {
            var db = new ParallelDb();
            var query = db.Select().From(db.Select().From("table2").Intersect(db.Select().From("table3").Skip(3)))
                .Take(3);
            Assert.AreEqual("SELECT * FROM (SELECT * FROM table2 INTERSECT (SELECT * FROM table3 OFFSET 3)) LIMIT 3",
                query.ToString());
        }

        [TestMethod]
        public void InsertQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Insert().Into("table1").Values(1, "2", true);
            Assert.AreEqual("INSERT INTO table1 VALUES (1, '2', True)", query.ToString());
        }

        [TestMethod]
        public void CreateQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Create().Table("table1").AddColumn("a", typeof(int)).AddColumn("b", typeof(string))
                .AddColumn("c", typeof(bool));
            Assert.AreEqual("CREATE TABLE table1 (a Int32 NOT NULL, b String NOT NULL, c Boolean NOT NULL)",
                query.ToString());
        }

        [TestMethod]
        public void DeleteQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Delete().From("table1");
            Assert.AreEqual("DELETE FROM table1", query.ToString());
        }

        [TestMethod]
        public void DropQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Drop().Table("table1");
            Assert.AreEqual("DROP TABLE table1", query.ToString());
        }

        [TestMethod]
        public void InsertQueryTest2()
        {
            var db = new ParallelDb();
            var query = db.Insert().Into("table1").Values(Default, "2", true).Values(2, "3", false);
            Assert.AreEqual("INSERT INTO table1 VALUES (DEFAULT, '2', True), (2, '3', False)", query.ToString());
        }

        // TODO: add tests that test COLUMNS()
        [TestMethod]
        public void UpdateQueryTest1()
        {
            var db = new ParallelDb();
            var query = db.Update().Table("table1").Set(row => row["a"] = 1).Set(row => row["b"] = "1")
                .Set(row => row["a"] = true).Where(row => row["b"] == "2");
            query.Table("table1").Set(row => row["a"] = 1).Set(row => row["b"] = 2).Where(row => row["a"] == 1);
            // Assert.AreEqual("UPDATE table1 SET a = 1, b = '2' WHERE a = 1", query.ToString());
        }
    }
}

[TestClass]
public class DependencyManagerTest
{
    [TestMethod]
    public void SimpleTest1()
    {
        int ComputeFirst(ConcurrentDictionary<int, dynamic?> _)
        {
            Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000);
            Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
            return 1;
        }

        int ComputeSecond(ConcurrentDictionary<int, dynamic?> _)
        {
            Console.WriteLine($"Starting operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000);
            Console.WriteLine($"Ending operation Thread ID {Thread.CurrentThread.ManagedThreadId}");
            return 2;
        }

        int ComputeThird(ConcurrentDictionary<int, dynamic?> dict)
        {
            if (dict[1] is null || dict[2] is null)
            {
                throw new Exception("Dependency not met");
            }

            int a = dict[1];
            int b = dict[2];
            return a + b * 10;
        }

        DependencyManager dm = new DependencyManager();
        dm.AddOperation(1, ComputeFirst);
        dm.AddOperation(2, ComputeSecond);
        dm.AddOperation(3, ComputeThird, 1, 2);


        var start = DateTimeOffset.Now;
        var res = dm.GetResults();
        var end = DateTimeOffset.Now;
        Assert.AreEqual(1, res.Count);
        Assert.AreEqual(21, res[3]);
        Assert.IsTrue(end - start < TimeSpan.FromSeconds(1.5));
    }

    // TODO: add tests for cycles
}

[TestClass]
public class QueryVisitorTest
{
    [TestMethod]
    public void CreateTableTest1()
    {
        var sql = @"
CREATE TABLE IF NOT EXISTS Persons (
    PersonID int DEFAULT 1,
    LastName TEXT NOT NULL,
    FirstName TeXt DEFAULT 'John',
    Address TEXT NOT NULL DEFAULT '1',
    City TEXT
);";
        var db = new ParallelDb();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(CreateTableQuery));
        Assert.AreEqual(
            "CREATE TABLE IF NOT EXISTS Persons (PersonID Int32, LastName String NOT NULL, FirstName String DEFAULT 'John', Address String NOT NULL DEFAULT '1', City String)",
            query.ToString());
    }

    [TestMethod]
    public void InsertTest1()
    {
        var sql =
            @"INSERT INTO Persons (PersonID, LastName, FirstName, Address, City) VALUES (1, 'John', 'Doe', '1', '1'), (1, 'John', 'Doe', '1', '1');";

        var db = new ParallelDb();
        db.Create().Table("Persons").AddColumn("PersonID", typeof(int)).AddColumn("LastName", typeof(string))
            .AddColumn("FirstName", typeof(string)).AddColumn("Address", typeof(string))
            .AddColumn("City", typeof(string))
            .Execute();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(InsertQuery));
        Assert.AreEqual(
            "INSERT INTO Persons (PersonID, LastName, FirstName, Address, City) VALUES (1, 'John', 'Doe', '1', '1'), (1, 'John', 'Doe', '1', '1')",
            query.ToString());
    }

    [TestMethod]
    public void UpdateTest1()
    {
        var sql = @"UPDATE Persons SET FirstName = 'John', LastName = 'Doe' WHERE PersonID = 1;";
        var db = new ParallelDb();
        db.Create().Table("Persons").AddColumn("PersonID", typeof(int)).AddColumn("LastName", typeof(string))
            .AddColumn("FirstName", typeof(string)).AddColumn("Address", typeof(string))
            .AddColumn("City", typeof(string))
            .Execute();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(UpdateQuery));
        // Assert.AreEqual("UPDATE Persons SET FirstName = 'John', LastName = 'Doe' WHERE PersonID = 1", query.ToString());
    }

    [TestMethod]
    public void DeleteTest1()
    {
        var sql = @"DELETE FROM Persons WHERE PersonID = 1;";
        var db = new ParallelDb();
        db.Create().Table("Persons").AddColumn("PersonID", typeof(int)).AddColumn("LastName", typeof(string))
            .AddColumn("FirstName", typeof(string)).AddColumn("Address", typeof(string))
            .AddColumn("City", typeof(string))
            .Execute();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(DeleteQuery));
        // Assert.AreEqual("DELETE FROM Persons WHERE PersonID = 1", query.ToString());
    }

    [TestMethod]
    public void SelectTest1()
    {
        var sql = @"SELECT * FROM Persons;";
        var db = new ParallelDb();
        db.Create().Table("Persons").AddColumn("PersonID", typeof(int)).AddColumn("LastName", typeof(string))
            .AddColumn("FirstName", typeof(string)).AddColumn("Address", typeof(string))
            .AddColumn("City", typeof(string))
            .Execute();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(SelectQuery));
        // Assert.AreEqual("SELECT * FROM Persons", query.ToString());
    }

    [TestMethod]
    public void DropTableTest1()
    {
        var sql = @"DROP TABLE Persons;";
        var db = new ParallelDb();
        db.Create().Table("Persons").AddColumn("PersonID", typeof(int)).AddColumn("LastName", typeof(string))
            .AddColumn("FirstName", typeof(string)).AddColumn("Address", typeof(string))
            .AddColumn("City", typeof(string))
            .Execute();
        var query = db.GetQuery(sql);
        Assert.IsInstanceOfType(query, typeof(DropTableQuery));
        Assert.AreEqual("DROP TABLE Persons", query.ToString());
    }
}