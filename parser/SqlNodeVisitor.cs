using Antlr4.Runtime.Misc;

namespace Parser;

using SqlParser;

public class SqlNodeVisitor : SQLiteParserBaseVisitor<object>
{
    public override object VisitSelect_stmt([NotNull] SQLiteParser.Select_stmtContext context)
    {
        // SqlNode node = new SelectNode();
        var coreContext = context.select_core()[0];
        Console.WriteLine("VisitSelect_core");

        context.select_core()[0].where_clause();

        context.select_core()[0].group_by_clause();
        // context.children[0].Accept(this);
        // context.select_core()[0].expr()[0];
        // context.limit_stmt().expr()[0].literal_value().NUMERIC_LITERAL();
        // context.limit_stmt().OFFSET_();

        return VisitChildren(context);
    }
}