namespace Domain.Types;

public class ParsedQuery
{
    public string SelectClause { get; set; } = string.Empty;
    public string FromClause { get; set; } = string.Empty;
    public string FromAlias { get; set; } = string.Empty;
    public string WhereClause { get; set; } = string.Empty;
    public List<(string Table, string Alias, string Condition)> Joins { get; private set; } = [];
    public Dictionary<string, string> TableAliases { get; private set; } = [];
}
