namespace Domain.Database;

public static class Keywords
{
    public const string SELECT = "SELECT";
    public const string FROM = "FROM";
    public const string WHERE = "WHERE";
    public const string JOIN = "JOIN";
    public const string ON = "ON";
    public const string AND = "AND";

    public const string OPEN_PARENTHESIS = "(";
    public const string CLOSE_PARENTHESIS = ")";
    public const string ASTERISK = "*";

    public const string EQUAL = "=";
    public const string GREATER_THAN = ">";
    public const string LESS_THAN = "<";
    public const string GREATER_THAN_OR_EQUAL = ">=";
    public const string LESS_THAN_OR_EQUAL = "<=";
    public const string NOT_EQUAL = "<>";

    public static readonly HashSet<string> Operators =
    [
        EQUAL,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        NOT_EQUAL,
    ];

    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        SELECT,
        FROM,
        WHERE,
        JOIN,
        ON,
        AND,
    };

    public static bool IsReserved(string word) => ReservedWords.Contains(word.ToUpperInvariant());
}
