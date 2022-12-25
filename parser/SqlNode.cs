using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ConsoleTables;
using Parser;
using SqlParser;

namespace Parser;

public class SqlNode
{
}

public class ExpressionNode : SQLiteParser.ExprContext
{
    public ExpressionNode(ParserRuleContext parent, int invokingState) : base(parent, invokingState)
    {
    }

    // Implement computation of the expression
    public bool IsBinary()
    {
        return GetChild(1) != null && GetChild(1) is not SQLiteParser.ExprContext &&
               GetChild(0) is SQLiteParser.ExprContext &&
               GetChild(2) is SQLiteParser.ExprContext;
    }

    public bool IsConstant()
    {
        return literal_value() is not null;
    }
}

public class SourceNode : SqlNode
{
    public readonly SQLiteParser.Table_or_subqueryContext Source;

    public SourceNode(SQLiteParser.Table_or_subqueryContext source)
    {
        Source = source;
    }
}

internal class SelectNode : SqlNode
{
    private readonly List<SqlNode> _columns;
    private readonly SourceNode _from;
    private readonly bool _distinct;
    private readonly ExpressionNode? _where;
    private readonly List<SqlNode> _groupBy;
    private readonly SqlNode? _having;
    private readonly List<SqlNode> _orderBy;
    private readonly SqlNode? _limit;
    private readonly SqlNode? _offset;

    public SelectNode(List<SqlNode> columns, SourceNode from, bool distinct = false, ExpressionNode? where = null,
        List<SqlNode>? groupBy = null, SqlNode? having = null, List<SqlNode>? orderBy = null, SqlNode? limit = null,
        SqlNode? offset = null)
    {
        _columns = columns;
        _from = from;
        _distinct = distinct;
        _where = where;
        _groupBy = groupBy ?? new List<SqlNode>();
        _having = having;
        _orderBy = orderBy ?? new List<SqlNode>();
        _limit = limit;
        _offset = offset;
    }

    public bool Distinct => _distinct;

    public List<SqlNode> Columns => _columns;

    public SqlNode? From => _from;

    // public SqlNode? Where => _where;

    public List<SqlNode> GroupBy => _groupBy;

    public SqlNode? Having => _having;

    public List<SqlNode> OrderBy => _orderBy;

    public SqlNode? Limit => _limit;

    public SqlNode? Offset => _offset;
}

class TableNode : SqlNode
{
    public readonly string Name;

    public TableNode(string name)
    {
        Name = name;
    }
}

class ColumnNode : SqlNode
{
    public readonly string Name;

    public ColumnNode(string name)
    {
        Name = name;
    }
}

class CompoundOperatorNode : SqlNode
{
    public readonly string Operator;

    public CompoundOperatorNode(string @operator)
    {
        Operator = @operator;
    }
}

class CompoundOperator
{
    
}

public class SelectResult : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < 6; i++)
        {
            yield return i * i;
        }
    }
}