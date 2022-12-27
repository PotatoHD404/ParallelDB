namespace Api;

public class SqlResponse
{
    public string[] Columns { get; set; }
    public dynamic[][] Rows { get; set; }
    public string SyntaxTree { get; set; }
    
    public string QueryTree { get; set; }
    
    public string PlannerTree { get; set; }
}