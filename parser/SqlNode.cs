using System.Text;
using Antlr4.Runtime.Tree;
using SqlParser;

namespace Parser;

public class SqlNode
{
}

public class SqlExpression : SqlNode
{
    public readonly SQLiteParser.ExprContext Expression;
    public SqlExpression(SQLiteParser.ExprContext expression)
    {
        Expression = expression;
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
    private readonly SqlExpression? _where;
    private readonly List<SqlNode> _groupBy;
    private readonly SqlNode? _having;
    private readonly List<SqlNode> _orderBy;
    private readonly SqlNode? _limit;
    private readonly SqlNode? _offset;

    public SelectNode(List<SqlNode> columns, SourceNode from, bool distinct = false, SqlExpression? where = null,
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

    public SqlNode? Where => _where;

    public List<SqlNode> GroupBy => _groupBy;

    public SqlNode? Having => _having;

    public List<SqlNode> OrderBy => _orderBy;

    public SqlNode? Limit => _limit;

    public SqlNode? Offset => _offset;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ");
        if (Distinct)
        {
            sb.Append("DISTINCT ");
        }

        sb.Append(string.Join(", ", Columns));
        sb.Append(" FROM ");
        sb.Append(From);
        if (Where != null)
        {
            sb.Append(" WHERE ");
            sb.Append(Where);
        }

        sb.Append(" GROUP BY ");
        sb.Append(string.Join(", ", GroupBy));
        if (Having != null)
        {
            sb.Append(" HAVING ");
            sb.Append(Having);
        }

        sb.Append(" ORDER BY ");
        sb.Append(string.Join(", ", OrderBy));


        if (Limit != null)
        {
            sb.Append(" LIMIT ");
            sb.Append(Limit);
        }

        if (Offset != null)
        {
            sb.Append(" OFFSET ");
            sb.Append(Offset);
        }

        return sb.ToString();
    }
}

public class ColumnNode : SqlNode
{
    public string Name { get; set; }
}

public class TableNode : SqlNode
{
    public string Name { get; set; }
}

public class WhereNode : SqlNode
{
    public string Column { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
}