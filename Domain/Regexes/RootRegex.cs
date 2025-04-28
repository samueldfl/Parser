using System.Text.RegularExpressions;

namespace Domain.Regexes;

public static partial class RootRegex
{
    [GeneratedRegex(@"SELECT (.+?) FROM", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex SelectRegex();

    [GeneratedRegex(@"FROM (.+?)( WHERE| JOIN|$)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex FromRegex();

    [GeneratedRegex(@"WHERE (.+)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex WhereRegex();

    [GeneratedRegex(@"JOIN (.+?) ON (.+?)( WHERE| JOIN|$)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex JoinRegex();
}
