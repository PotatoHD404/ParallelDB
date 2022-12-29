namespace ParallelDB.Tables;

public interface IRow
{
    dynamic? this[string columnName] { get; set; }

    dynamic? this[int columnIndex] { get; set; }
}