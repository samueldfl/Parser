namespace Domain.Database;

public static class Keywords
{
    public const string SELECT = "SELECT";
    public const string FROM = "FROM";
    public const string WHERE = "WHERE";
    public const string JOIN = "JOIN";
    public const string ON = "ON";
    public const string IN = "IN";
    public const string AND = "AND";
    public const string OR = "OR";

    public const string INNER = "INNER";
    public const string LEFT = "LEFT";
    public const string RIGHT = "RIGHT";
    public const string FULL = "FULL";
    public const string GROUP = "GROUP";
    public const string BY = "BY";
    public const string ORDER = "ORDER";
    public const string HAVING = "HAVING";
    public const string AS = "AS";
    public const string LIMIT = "LIMIT";
    public const string OFFSET = "OFFSET";
    public const string UNION = "UNION";

    public const string OPEN_PARENTHESIS = "(";
    public const string CLOSE_PARENTHESIS = ")";
    public const string ASTERISK = "*";

    private const string EQUAL = "=";
    public const string GREATER_THAN = ">";
    public const string LESS_THAN = "<";
    public const string GREATER_THAN_OR_EQUAL = ">=";
    public const string LESS_THAN_OR_EQUAL = "<=";
    private const string NOT_EQUAL = "<>";

    public static readonly HashSet<string> Operators =
    [
        EQUAL,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        NOT_EQUAL,
        IN,
    ];

    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        SELECT,
        FROM,
        WHERE,
        JOIN,
        ON,
        AND,
        OR,
        INNER,
        LEFT,
        RIGHT,
        FULL,
        GROUP,
        BY,
        ORDER,
        HAVING,
        AS,
        LIMIT,
        OFFSET,
        UNION,
    };

    public static bool IsReserved(string word) => ReservedWords.Contains(word.ToUpperInvariant());
}
