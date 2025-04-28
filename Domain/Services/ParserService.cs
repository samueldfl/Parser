using System.Text.RegularExpressions;
using Domain.Database;
using Domain.Regexes;
using Domain.Types;

namespace Domain.Services;

public static class ParserService
{
    public static Result<ParsedQuery> Parse(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return Result<ParsedQuery>.Failure("Query is empty.");

        var parsedQuery = new ParsedQuery();

        var selectMatch = RootRegex.SelectRegex().Match(sql);
        if (selectMatch.Success)
            parsedQuery.SelectClause = selectMatch.Groups[1].Value.Trim();

        var fromMatch = RootRegex.FromRegex().Match(sql);
        if (fromMatch.Success)
            parsedQuery.FromClause = fromMatch.Groups[1].Value.Trim();

        var whereMatch = RootRegex.WhereRegex().Match(sql);
        if (whereMatch.Success)
            parsedQuery.WhereClause = whereMatch.Groups[1].Value.Trim();

        var joinMatches = RootRegex.JoinRegex().Matches(sql);
        foreach (Match joinMatch in joinMatches)
        {
            var table = joinMatch.Groups[1].Value.Trim();
            var condition = joinMatch.Groups[2].Value.Trim();
            parsedQuery.Joins.Add((table, condition));
        }

        if (!Schema.Tables.Contains(parsedQuery.FromClause))
            return Result<ParsedQuery>.Failure(
                $"Table '{parsedQuery.FromClause}' does not exist in the schema."
            );

        foreach (var join in parsedQuery.Joins.Where(join => !Schema.Tables.Contains(join.Table)))
        {
            return Result<ParsedQuery>.Failure(
                $"Table '{join.Table}' from JOIN does not exist in the schema."
            );
        }

        var selectFields = parsedQuery.SelectClause.Split(',').Select(f => f.Trim()).ToList();

        if (selectFields is not ["*"])
        {
            if (!Schema.Columns.TryGetValue(parsedQuery.FromClause, out var availableColumns))
                return Result<ParsedQuery>.Failure(
                    $"Table '{parsedQuery.FromClause}' does not have registered columns in the schema."
                );

            foreach (var field in selectFields.Where(field => !availableColumns.Contains(field)))
            {
                return Result<ParsedQuery>.Failure(
                    $"Field '{field}' does not exist in table '{parsedQuery.FromClause}'."
                );
            }
        }

        if (string.IsNullOrEmpty(parsedQuery.WhereClause))
            return Result<ParsedQuery>.Success(parsedQuery);

        var tokens = parsedQuery
            .WhereClause.Replace("(", " ( ")
            .Replace(")", " ) ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();

        foreach (
            var token in tokens
                .Where(token =>
                    !Keywords.Operators.Contains(token)
                    && !Keywords.LogicalOperators.Contains(token)
                )
                .Where(token =>
                    token != Keywords.OPEN_PARENTHESIS && token != Keywords.CLOSE_PARENTHESIS
                )
        )
        {
            if (!Schema.Columns.TryGetValue(parsedQuery.FromClause, out var availableColumns))
                return Result<ParsedQuery>.Failure(
                    $"Table '{parsedQuery.FromClause}' does not have columns for validation."
                );

            if (!availableColumns.Contains(token) && !IsLiteral(token))
            {
                return Result<ParsedQuery>.Failure(
                    $"Unknown field or invalid token '{token}' in WHERE clause."
                );
            }

            if (!availableColumns.Contains(token))
                continue;

            var index = tokens.IndexOf(token);

            if (index + 2 >= tokens.Count)
                continue;

            var operatorToken = tokens[index + 1];
            var valueToken = tokens[index + 2];

            if (!Keywords.Operators.Contains(operatorToken))
                continue;

            if (!Schema.ColumnTypes.TryGetValue(parsedQuery.FromClause, out var tableColumns))
                continue;

            if (!tableColumns.TryGetValue(token, out var fieldType))
                continue;

            if (!IsCompatible(fieldType, valueToken))
            {
                return Result<ParsedQuery>.Failure(
                    $"Type mismatch: Field '{token}' expects {fieldType}, but got '{valueToken}'."
                );
            }
        }

        return Result<ParsedQuery>.Success(parsedQuery);
    }

    private static bool IsLiteral(string token)
    {
        return int.TryParse(token, out _) || (token.StartsWith('\'') && token.EndsWith('\''));
    }

    private static bool IsCompatible(string fieldType, string valueToken)
    {
        return fieldType switch
        {
            "int" => int.TryParse(valueToken, out _),
            "decimal" => decimal.TryParse(valueToken, out _),
            "string" => valueToken.StartsWith('\'') && valueToken.EndsWith('\''),
            "date" => valueToken.StartsWith('\'')
                && valueToken.EndsWith('\'')
                && DateTime.TryParse(fieldType, out _),
            _ => false,
        };
    }
}
