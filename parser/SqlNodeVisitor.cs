using Antlr4.Runtime.Misc;

namespace Parser;

using SqlParser;

public class SqlNodeVisitor : SQLiteParserBaseVisitor<object>
{
    public override object VisitSelect_stmt([NotNull] SQLiteParser.Select_stmtContext context)
    {
        Console.WriteLine("VisitSelect_core");
        Console.WriteLine(context.GetText());
        Console.WriteLine(context.select_core()[0].SELECT_());
        context.select_core()[0].SELECT_();
        Console.WriteLine(context.select_core()[0].DISTINCT_() == null);
        context.select_core()[0].result_column();
        context.select_core()[0].table_or_subquery();
        context.select_core()[0].join_clause();
        context.select_core()[0].WHERE_();
        context.select_core()[0].expr();
        context.select_core()[0].GROUP_();
        context.select_core()[0].BY_();
        context.select_core()[0].expr();
        context.select_core()[0].HAVING_();
        context.children[0].Accept(this);
        // context.select_core()[0].expr()[0];
        // context.limit_stmt().expr()[0].literal_value().NUMERIC_LITERAL();
        // context.limit_stmt().OFFSET_();

        return VisitChildren(context);
    }
}