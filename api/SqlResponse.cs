namespace Api;

public class SqlResponse
{
    public string[] Columns { get; set; }
    public dynamic[][] Rows { get; set; }
    public string SyntaxTree { get; set; }
}