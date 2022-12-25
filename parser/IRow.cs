namespace Parser;

public interface IRow
{
    dynamic this[string columnName] { get; set; }
}