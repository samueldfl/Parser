namespace Domain.Database;

public static class Keywords
{
    public const string SELECT = "SELECT";
    public const string FROM = "FROM";
    public const string WHERE = "WHERE";
    public const string JOIN = "JOIN";
    public const string ON = "ON";

    private const string EQUAL = "=";
    private const string GREATER_THAN = ">";
    private const string LESS_THAN = "<";
    private const string GREATER_THAN_OR_EQUAL = ">=";
    private const string LESS_THAN_OR_EQUAL = "<=";
    private const string NOT_EQUAL = "<>";
    private const string AND = "AND";
    private const string OR = "OR";

    public const string OPEN_PARENTHESIS = "(";
    public const string CLOSE_PARENTHESIS = ")";
    public const string ASTERISK = "*";

    public static readonly HashSet<string> Operators =
    [
        EQUAL,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        NOT_EQUAL,
    ];

    public static readonly HashSet<string> LogicalOperators = [AND, OR];
}
