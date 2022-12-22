using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SqlParser;

namespace Parser;

public class GraphvizVisitor : SQLiteParserBaseVisitor<object?>
{
    private readonly StringBuilder _sb = new();
    private bool _ended = true;

    public string GetGraph()
    {
        if (!_ended)
            _sb.Append("}");
        return _sb.ToString();
    }

    private void Clear()
    {
        _ended = false;
        _sb.Clear();
    }

    // override Accept method
    public override object? Visit([NotNull] IParseTree tree)
    {
        if (tree.Parent == null)
        {
            Clear();
            _sb.AppendLine("digraph G {");
            _sb.AppendLine("   node [fontsize=14 shape=plain ordering=\"in\"];");
            // _sb.AppendLine("   edge [fontsize=14];");
        }

        if (tree is ParserRuleContext n)
        {
            // _sb.AppendLine($"node{n.GetHashCode()} [label=\"{tree.GetType().Name}\"];");
            _sb.AppendLine($"   \"{n.GetHashCode()}\" [label=\"{SQLiteParser.ruleNames[n.RuleIndex]}\"];");
        }
        else if (tree is ITerminalNode t)
        {
            _sb.Append($"   \"{t.GetHashCode()}\" [label=\"");

            if (SQLiteParser.DefaultVocabulary.GetSymbolicName(t.Symbol.Type) != t.GetText())
            {
                _sb.Append($"{SQLiteParser.DefaultVocabulary.GetSymbolicName(t.Symbol.Type)}:");
            }

            _sb.AppendLine($"{t.GetText()}\"];");
        }

        // Console.WriteLine(tree is ParserRuleContext);
        return base.Visit(tree);
    }

    public override object? VisitChildren(IRuleNode node)
    {
        if (node is ParserRuleContext n)
        {
            for (var i = 0; i < n.ChildCount; i++)
            {
                var child = n.GetChild(i);

                _sb.AppendLine($"   \"{n.GetHashCode()}\" -> \"{child.GetHashCode()}\";");
                Visit(child);
            }
        }

        return null;
    }
}