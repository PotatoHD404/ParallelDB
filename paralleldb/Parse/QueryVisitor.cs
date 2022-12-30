using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ParallelDB.Queries;
using ParallelDB.Tables;
using SqlParser;

namespace ParallelDB.Parse;

internal class NotNullType
{
}

internal class DefaultNode
{
    public dynamic Value { get; set; }
}

public class QueryVisitor : SQLiteParserBaseVisitor<dynamic?>
{
    private static readonly NotNullType NotNull = new();
    private readonly ParallelDb _db;

    public QueryVisitor(ParallelDb db)
    {
        _db = db;
    }

    public override dynamic? VisitParse(SQLiteParser.ParseContext context)
    {
        if (context.sql_stmt_list().Length == 0)
        {
            return null;
        }

        if (context.sql_stmt_list().Length > 1)
        {
            throw new NotSupportedException("Multiple statements are not supported");
        }

        return VisitSql_stmt_list(context.sql_stmt_list()[0]);
    }

    public override dynamic? VisitSql_stmt_list([NotNull] SQLiteParser.Sql_stmt_listContext context)
    {
        var res = context.sql_stmt();
        if (res.Length > 1)
        {
            throw new NotSupportedException("Multiple statements are not supported");
        }

        if (res.Length == 0)
        {
            throw new NotSupportedException("Empty query");
        }

        return VisitSql_stmt(context.sql_stmt()[0]);
    }

    public override dynamic? VisitSql_stmt([NotNull] SQLiteParser.Sql_stmtContext context)
    {
        if (context.create_table_stmt() != null)
        {
            return VisitCreate_table_stmt(context.create_table_stmt());
        }

        if (context.insert_stmt() != null)
        {
            return VisitInsert_stmt(context.insert_stmt());
        }

        if (context.select_stmt() != null)
        {
            return Visit(context.select_stmt());
        }

        if (context.update_stmt() != null)
        {
            return Visit(context.update_stmt());
        }

        if (context.delete_stmt() != null)
        {
            return Visit(context.delete_stmt());
        }

        if (context.drop_stmt() != null)
        {
            return Visit(context.delete_stmt());
        }

        throw new NotSupportedException($"Statement {context.GetText()} is not supported");
    }
    
    public override dynamic VisitInsert_stmt([NotNull] SQLiteParser.Insert_stmtContext context)
    {
        string tableName = context.table_name().GetText();
        var res = _db.Insert();
        Table? table = _db.GetTable(tableName);
        if (table is null)
        {
            throw new Exception($"Table {tableName} does not exist");
        }
        res.Into(tableName);
        var columns = context.column_name().Select(x => x.GetText()).ToArray();
        foreach (string col in columns)
        {
            table.ColumnIndex(col);
        }
        var values = context.expr().Select(x => Visit(x)).ToArray();
        res.Values(
        return null;
    }

    public override dynamic VisitCreate_table_stmt([NotNull] SQLiteParser.Create_table_stmtContext context)
    {
        var tableName = context.table_name().GetText();
        var res = _db.Create();
        res.tableName = tableName;
        res.columns = context.column_def().Select(Visit).Cast<Column>().ToDictionary(x => x.Name, x => x);
        res.ifNotExists = context.IF() is not null && context.NOT() is not null && context.EXISTS() is not null;
        return res;
    }

    public override dynamic VisitColumn_def([NotNull] SQLiteParser.Column_defContext context)
    {
        var name = context.column_name().GetText();
        var typeName = context.type_name().GetText();

        Type type = ToClrType(typeName);
        if (type is null)
        {
            throw new NotSupportedException($"Type {typeName} is not supported");
        }

        // visit column constraints
        var constraints = context.column_constraint().Select(Visit).ToList();
        var isNullable = !constraints.OfType<NotNullType>().Any();
        var defaultValue = constraints.OfType<DefaultNode>().FirstOrDefault()?.Value;
        if (defaultValue is null)
        {
            return new Column(name, type, isNullable);
        }

        return new Column(name, type, isNullable, true, defaultValue);
    }

    private static Type ToClrType(string typeName)
    {
        var type = typeName.ToUpper() switch
        {
            "TINYINT" => typeof(byte),
            "SMALLINT" => typeof(short),
            "MEDIUMINT" => typeof(int),
            "INT" => typeof(int),
            "INTEGER" => typeof(int),
            "BIGINT" => typeof(long),
            "FLOAT" => typeof(float),
            "DOUBLE" => typeof(double),
            "REAL" => typeof(double),
            "BIT" => typeof(bool),
            "BOOL" => typeof(bool),
            "BOOLEAN" => typeof(bool),
            "TEXT" => typeof(string),
            "BLOB" => typeof(byte[]),
            "TIME" => typeof(TimeSpan),
            "DATE" => typeof(DateTime),
            "DATETIME" => typeof(DateTime),
            "TIMESTAMP" => typeof(DateTime),
            _ => throw new NotSupportedException()
        };

        return type;
    }

    public override dynamic VisitColumn_constraint([NotNull] SQLiteParser.Column_constraintContext context)
    {
        var not = context.NOT() is not null;
        var @null = context.NULL() is not null;
        if (not && @null)
        {
            return NotNull;
        }

        var @default = context.DEFAULT() is not null;
        dynamic? value;
        if (context.literal_value() is not null)
        {
            value = GetValue(context.literal_value());
        }
        else if (context.signed_number() is not null)
        {
            value = GetValue(context.signed_number());
        }
        else
        {
            throw new NotSupportedException("Default value is not supported");
        }


        return new DefaultNode { Value = value };
    }

    public dynamic GetValue([NotNull] ParserRuleContext context)
    {
        switch (context)
        {
            case SQLiteParser.Literal_valueContext literalValueContext:
                return VisitLiteral_value(literalValueContext);
            case SQLiteParser.Signed_numberContext signedNumberContext:
                return VisitSigned_number(signedNumberContext);
            default:
                throw new NotSupportedException("Value is not supported");
        }
    }

    public override dynamic? VisitLiteral_value([NotNull] SQLiteParser.Literal_valueContext context)
    {
        if (context.NUMERIC_LITERAL() is not null)
        {
            return context.NUMERIC_LITERAL() switch
            {
                { } n when n.GetText().Contains('.') => double.Parse(n.GetText()),
                { } n => int.Parse(n.GetText())
            };
        }

        if (context.STRING_LITERAL() is not null)
        {
            var res = context.STRING_LITERAL().GetText();
            return res[1..^1];
        }

        if (context.NULL() is not null)
        {
            return null;
        }

        if (context.TRUE() is not null)
        {
            return true;
        }

        if (context.FALSE() is not null)
        {
            return false;
        }

        if (context.BLOB_LITERAL() is not null)
        {
            throw new NotSupportedException("BLOB is not supported");
        }


        throw new NotSupportedException("Literal value is not supported");
    }
}