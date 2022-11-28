using Antlr4.Runtime.Misc;

namespace Parser;

using SqlParser;

public class ParallelDbVisitor : SQLiteParserBaseVisitor<object>
{
    public override object VisitTable_name([NotNull] SQLiteParser.Table_nameContext context)
    {
        Console.WriteLine(context.GetText());
        return context.GetText();
    }
    
    public override object Visit
}

