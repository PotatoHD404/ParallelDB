using System.Linq.Expressions;
using Antlr4.Runtime.Misc;

namespace Parser;

using SqlParser;

public class SqlNodeVisitor : SQLiteParserBaseVisitor<SqlNode?>
{
    public override SqlNode? VisitSelect_stmt([NotNull] SQLiteParser.Select_stmtContext context)
    {
        // SqlNode node = new SelectNode();
        List<SelectNode> selects = context.select_core().Select(VisitSelect_core).Where(el => true).OfType<SelectNode>().ToList();
        
        

        Console.WriteLine("VisitSelect_core");

        context.select_core()[0].where_clause();

        context.select_core()[0].group_by_clause();
        // context.children[0].Accept(this);
        // context.select_core()[0].expr()[0];
        // context.limit_stmt().expr()[0].literal_value().NUMERIC_LITERAL();
        // context.limit_stmt().OFFSET_();
        return null;
    }

    public override SqlNode? VisitSelect_core([NotNull] SQLiteParser.Select_coreContext context)
    {
        Console.WriteLine("VisitSelect_core");
        return null;
    }
}