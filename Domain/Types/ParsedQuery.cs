namespace Domain.Types;

public class ParsedQuery
{
    public string SelectClause { get; set; } = string.Empty;

    public string FromClause { get; set; } = string.Empty;

    public List<(string Table, string Condition)> Joins { get; private set; } = [];

    public string WhereClause { get; set; } = string.Empty;
}
