﻿using System.Text;
using ParallelDB.Parse;
using ParallelDB.Tables;

namespace ParallelDB.Queries;

public class SelectQuery : IQuery
{
    private ParallelDb _db;
    internal List<string> project;
    internal List<Tuple<object, Func<IRow, IRow, bool>>> join;
    internal List<Func<IRow, bool>> where;
    internal List<object> from;
    internal List<object> union;
    internal List<object> unionAll;
    internal List<object> intersect;
    internal List<object> except;
    internal bool distinct;
    internal int? take;
    internal int? skip;

    public SelectQuery(ParallelDb db)
    {
        _db = db;
        project = new List<string>();
        join = new List<Tuple<object, Func<IRow, IRow, bool>>>();
        where = new List<Func<IRow, bool>>();
        from = new List<object>();
        union = new List<object>();
        unionAll = new List<object>();
        intersect = new List<object>();
        except = new List<object>();
        distinct = false;
        take = null;
        skip = null;
    }

    public SelectQuery Project(params string[] columns)
    {
        project.AddRange(columns);
        return this;
    }

    public SelectQuery From(string table)
    {
        from.Add(table);
        return this;
    }

    public SelectQuery From(SelectQuery table)
    {
        from.Add(table);
        return this;
    }

    public SelectQuery Join(string table, Func<IRow, IRow, bool> condition)
    {
        join.Add(new Tuple<object, Func<IRow, IRow, bool>>(table, condition));
        return this;
    }

    public SelectQuery Join(SelectQuery table, Func<IRow, IRow, bool> condition)
    {
        join.Add(new Tuple<object, Func<IRow, IRow, bool>>(table, condition));
        return this;
    }

    public SelectQuery Where(Func<IRow, bool> condition)
    {
        where.Add(condition);
        return this;
    }

    public SelectQuery Union(string table)
    {
        union.Add(table);
        return this;
    }

    public SelectQuery Union(SelectQuery table)
    {
        union.Add(table);
        return this;
    }

    public SelectQuery UnionAll(string table)
    {
        unionAll.Add(table);
        return this;
    }

    public SelectQuery UnionAll(SelectQuery table)
    {
        unionAll.Add(table);
        return this;
    }

    public SelectQuery Intersect(string table)
    {
        intersect.Add(table);
        return this;
    }

    public SelectQuery Intersect(SelectQuery table)
    {
        intersect.Add(table);
        return this;
    }

    public SelectQuery Except(string table)
    {
        except.Add(table);
        return this;
    }

    public SelectQuery Except(SelectQuery table)
    {
        except.Add(table);
        return this;
    }

    public SelectQuery Distinct()
    {
        distinct = true;
        return this;
    }

    public SelectQuery Take(int count)
    {
        take = count;
        return this;
    }

    public SelectQuery Skip(int count)
    {
        skip = count;
        return this;
    }

    private StringBuilder TableToString(object obj)
    {
        StringBuilder sb = new StringBuilder();
        if (obj is SelectQuery obj1)
        {
            sb.Append("(");
            sb.Append(obj1);
            sb.Append(")");
        }
        else
        {
            sb.Append(obj);
        }

        return sb;
    }

    private StringBuilder JoinToString(Tuple<object, Func<IRow, IRow, bool>> obj)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("JOIN ");
        sb.Append(TableToString(obj.Item1));
        sb.Append(" ON ");
        sb.Append(obj.Item2);
        return sb;
    }

    // to string method
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ");
        if (distinct)
            sb.Append("DISTINCT ");
        if (project.Count == 0)
            sb.Append("*");
        else
            sb.Append(string.Join(", ", project));
        sb.Append(" FROM ");
        sb.Append(string.Join(", ", from.Select(TableToString)));
        if (join.Count > 0)
            sb.Append(" " + string.Join(" ", join.Select(JoinToString)));
        if (where.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", where));
        }

        if (union.Count > 0)
        {
            sb.Append(" UNION ");
            sb.Append(string.Join(" UNION ", union.Select(TableToString)));
        }

        if (unionAll.Count > 0)
        {
            sb.Append(" UNION ALL ");
            sb.Append(string.Join(" UNION ALL ", unionAll.Select(TableToString)));
        }

        if (intersect.Count > 0)
        {
            sb.Append(" INTERSECT ");
            sb.Append(string.Join(" INTERSECT ", intersect.Select(TableToString)));
        }

        if (except.Count > 0)
        {
            sb.Append(" EXCEPT ");
            sb.Append(string.Join(" EXCEPT ", except.Select(TableToString)));
        }

        if (take.HasValue)
        {
            sb.Append(" LIMIT ");
            sb.Append(take.Value);
        }

        if (skip.HasValue)
        {
            sb.Append(" OFFSET ");
            sb.Append(skip.Value);
        }

        return sb.ToString();
    }

    
    public string GetPlan()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("bgcolor= transparent;");
        sb.AppendLine("rankdir=BT;");
        // cycle that finds all from tables
        for (int i = 0; i < this.from.Count; i++)
        {
            if (from[i] is SelectQuery obj1)
            {
                
            }
            else
            {
                if (take.HasValue || skip.HasValue)
                {
                    sb.AppendLine($"{this.from[i]} -> Limit");
                }
                else
                {
                    sb.AppendLine($"{this.from[i]}");
                }
            }
        }
        // check if there is take or skip
        sb.AppendLine("}");
        return sb.ToString();
    }
}