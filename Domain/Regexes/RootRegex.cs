﻿using System.Text.RegularExpressions;

namespace Domain.Regexes;

public static partial class RootRegex
{
    [GeneratedRegex(@"SELECT (.+?) FROM", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex SelectRegex();

    [GeneratedRegex(@"WHERE (.+)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex WhereRegex();

    [GeneratedRegex(@"FROM\s+(\w+)(?:\s+AS\s+(\w+)|\s+(\w+))?", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex FromWithAliasRegex();

    [GeneratedRegex(
        @"JOIN\s+(\w+)(?:\s+(\w+))?\s+ON\s+(.+?)(?=\s+JOIN|\s+WHERE|$)",
        RegexOptions.IgnoreCase,
        "en-US"
    )]
    public static partial Regex JoinWithAliasRegex();
}
